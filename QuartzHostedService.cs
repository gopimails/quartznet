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
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<SharedJobSchedule> _jobSchedules;
        private readonly ILogger<QuartzHostedService> _logger;
        private IScheduler _scheduler;

        public QuartzHostedService(ISchedulerFactory schedulerFactory,
                                   IEnumerable<SharedJobSchedule> jobSchedules,
                                   ILogger<QuartzHostedService> logger,
                                   IJobFactory jobFactory)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobSchedules;
            _logger = logger;
            _jobFactory = jobFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("HostedService Started");
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            foreach(var jobSchedule in _jobSchedules)
            {
                var job = QuartzShared.CreateJob(jobSchedule);
                var trigger = QuartzShared.CreateTrigger(jobSchedule);

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
            _logger.LogWarning("HostedService Stopped");
        }
    }
}