using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace quartznet.jobs
{
    [DisallowConcurrentExecution]
    public class JobC : IJob
    {
        private readonly ILogger<JobC> _logger;
        private IList<byte[]> createLeakList = new List<byte[]>();
        public JobC(ILogger<JobC> logger)
        {
            _logger = logger;
            _logger.LogDebug("Constructed {job}", nameof(JobC));
        }
        public async Task Execute(IJobExecutionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            _logger.LogInformation("{Job} Started", nameof(JobC));
            try
            {
                createLeakList.Add(new byte[1024]);
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 500)), context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Job} Failed. \n Error : {Message} \n Skipping error.", nameof(JobC), ex.Message);
            }
            finally
            {
                _logger.LogDebug("{Job} Completed", nameof(JobC));
            }
        }
    }
}