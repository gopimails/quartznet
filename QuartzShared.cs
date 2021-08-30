using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace quartznet
{
    public class SharedJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        public SharedJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return serviceProvider.GetRequiredService<SharedJobRunner>();
        }

        public void ReturnJob(IJob job)
        {
        }
    }

    [DisallowConcurrentExecution]
    public class SharedJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        public SharedJobRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // var job = _serviceProvider.GetRequiredService(context.JobDetail.JobType) as IJob;
            // await job.Execute(context);
            int gcIdentifier;
            using (var scope = _serviceProvider.CreateScope())
            {
                var jobType = context.JobDetail.JobType;
                var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;
                gcIdentifier = GC.GetGeneration(job);
                Serilog.Log.Information("gc id {id}", gcIdentifier);
                await job.Execute(context);
                Serilog.Log.Information("Exec done");
                // job.Execute(context).Wait();
            }
            GC.Collect(gcIdentifier, GCCollectionMode.Forced);
            Serilog.Log.Information("GC done");
        }
    }

    public class SharedJobSchedule
    {
        public SharedJobSchedule(Type jobType, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
    }

    public static class QuartzShared
    {
        public static IJobDetail CreateJob(SharedJobSchedule schedule)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                    .Create(jobType)
                    .WithIdentity(jobType.FullName)
                    .WithDescription(jobType.Name)
                    .Build();
        }
        public static ITrigger CreateTrigger(SharedJobSchedule schedule)
        {
            return TriggerBuilder
                    .Create()
                    .WithIdentity($"{schedule.JobType.FullName}.trigger")
                    .WithCronSchedule(schedule.CronExpression)
                    .WithDescription(schedule.CronExpression)
                    .Build();
        }
    }
}