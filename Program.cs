using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using quartznet.jobs;


namespace quartznet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                            .CreateLogger();
            try
            {
                Log.Information("QuartzNet Application Starting.");

                var configuration = new ConfigurationBuilder()
                                    .AddJsonFile("serilogsettings.json", optional: false, reloadOnChange: true)
                                    .Build();

                Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(configuration)
                                .CreateLogger();

                var host = new HostBuilder()
                            .ConfigureServices((hostContext, services) =>
                            {
                                services.AddSingleton<IJobFactory, SharedJobFactory>();
                                services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                                services.AddSingleton<SharedJobRunner>();
                                services.AddHostedService<QuartzHostedService>();

                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobA), cronExpression: "0 0/1 * * * ?")); // run every min
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobB), cronExpression: "0 0/1 * * * ?")); // run every min
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobC), cronExpression: "0 0/1 * * * ?")); // run every min
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobD), cronExpression: "0 0/2 * * * ?")); // run every 2 min
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobE), cronExpression: "0 0/2 * * * ?")); // run every 2 mins
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobF), cronExpression: "0 0/3 * * * ?")); // run every 3 mins
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobG), cronExpression: "0 0/1 * * * ?")); // run every min
                                services.AddSingleton(new SharedJobSchedule(jobType: typeof(JobH), cronExpression: "0 0/2 * * * ?")); // run every 2 min                  

                                services.AddTransient<JobA>();
                                services.AddTransient<JobB>();
                                services.AddTransient<JobC>();
                                services.AddTransient<JobD>();
                                services.AddTransient<JobE>();
                                services.AddTransient<JobF>();
                                services.AddTransient<JobG>();
                                services.AddTransient<JobH>();
                            })
                            .UseSerilog()
                            .UseConsoleLifetime()
                            .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "QuartzNet Application failed to start.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
