using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace quartznet.jobs
{
    [DisallowConcurrentExecution]
    public class JobH : IJob
    {
        private readonly ILogger<JobH> _logger;
        private IList<byte[]> createLeakList = new List<byte[]>();
        public JobH(ILogger<JobH> logger)
        {
            _logger = logger;
            _logger.LogDebug("Constructed {job}", nameof(JobH));
        }
        public async Task Execute(IJobExecutionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            _logger.LogInformation("{Job} Started", nameof(JobH));
            try
            {
                createLeakList.Add(new byte[1000000]);
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 500)), context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Job} Failed. \n Error : {Message} \n Skipping error.", nameof(JobH), ex.Message);
            }
            finally
            {
                _logger.LogDebug("{Job} Completed", nameof(JobH));
            }
        }
    }
}