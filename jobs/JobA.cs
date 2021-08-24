using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Quartz;

namespace quartznet.jobs
{
    [DisallowConcurrentExecution]
    public class JobA : IJob
    {
        private readonly ILogger<JobA> _logger;
        private IList<byte[]> createLeakList = new List<byte[]>();
        public JobA(ILogger<JobA> logger)
        {
            Log.Debug("Constructed {job}", nameof(JobA));
            this._logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            _logger.LogInformation("{Job} Started", nameof(JobA));
            try
            {
                createLeakList.Add(new byte[1024]);
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 500)), context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Job} Failed. \n Error : {Message} \n Skipping error.", nameof(JobA), ex.Message);
            }
            finally
            {
                _logger.LogDebug("{Job} Completed", nameof(JobA));
            }
        }
    }
}