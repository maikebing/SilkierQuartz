using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using QuartzHostedService;
using SampleQuartzHostedService.Jobs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SampleQuartzHostedService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOptions();
                    services.Configure<InjectProperty>(options=> { options.WriteText = "This is inject string"; });
                    services.UseQuartzHostedService()
                    .RegiserJob<HelloJob>(() =>
                    {
                        var result = new List<TriggerBuilder>();
                        result.Add(TriggerBuilder.Create()
                            .WithSimpleSchedule(x=>x.WithIntervalInSeconds(1).RepeatForever()));
                        result.Add(TriggerBuilder.Create()
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(2).RepeatForever()));
                        return result;
                    })
                    .RegiserJob<InjectSampleJob>(() =>
                    {
                        var result = new List<TriggerBuilder>();
                        result.Add(TriggerBuilder.Create()
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
                        return result;
                    });
                }).ConfigureQuartzHost();

            await hostBuilder.RunConsoleAsync();
        }
    }
}
