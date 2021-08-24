using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace quartznet.jobs
{
    [DisallowConcurrentExecution]
    public class JobG : IJob
    {
        private readonly ILogger<JobG> _logger;
        private IList<byte[]> createLeakList = new List<byte[]>();
        public JobG(ILogger<JobG> logger)
        {
            _logger = logger;
            _logger.LogDebug("Constructed {job}", nameof(JobG));
        }
        public async Task Execute(IJobExecutionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            _logger.LogInformation("{Job} Started", nameof(JobG));
            try
            {
                createLeakList.Add(new byte[1024]);
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 500)), context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Job} Failed. \n Error : {Message} \n Skipping error.", nameof(JobG), ex.Message);
            }
            finally
            {
                _logger.LogDebug("{Job} Completed", nameof(JobG));
            }
        }
    }
}