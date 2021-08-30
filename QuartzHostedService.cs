using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace quartznet
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IJobFactory jobFactory;
        private readonly IEnumerable<SharedJobSchedule> jobSchedules;
        private readonly ILogger<QuartzHostedService> logger;
        private IScheduler scheduler;

        public QuartzHostedService(ISchedulerFactory schedulerFactory,
                                   IEnumerable<SharedJobSchedule> jobSchedules,
                                   ILogger<QuartzHostedService> logger,
                                   IJobFactory jobFactory)
        {
            this.schedulerFactory = schedulerFactory;
            this.jobSchedules = jobSchedules;
            this.logger = logger;
            this.jobFactory = jobFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("HostedService Started");
            scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            scheduler.JobFactory = jobFactory;

            foreach(var jobSchedule in jobSchedules)
            {
                var job = QuartzShared.CreateJob(jobSchedule);
                var trigger = QuartzShared.CreateTrigger(jobSchedule);

                await scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {            
            await scheduler.Shutdown(cancellationToken);
            logger.LogWarning("HostedService Stopped");
        }

        //https://medium.com/@rainer_8955/gracefully-shutdown-c-apps-2e9711215f6d
        //https://www.kaggle.com/residentmario/best-practices-for-propagating-signals-on-docker
        //  https://github.com/moby/moby/issues/19300
        //https://docs.docker.com/engine/reference/builder/#stopsignal

        //Quartz should clean after itself. If you implemented a custom JobFactory be sure to Release your Jobs after execution.
        //https://stackoverflow.com/questions/39535420/quartz-net-scheduler-memory-leak

        //https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html#trying-out-the-application-and-adding-jobs


        
    }
}