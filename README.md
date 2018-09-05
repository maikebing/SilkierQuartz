#	QuartzHostedService

Wrapper above [Quartz.NET] (https://github.com/quartznet/quartznet) for .NET Core.

## Usage

1. Create Quartz-Job implements IJob interface

    ``` C#
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello");
            return Task.CompletedTask;
        }
    }
    ```
1. Call extension methode __UseQuartzHostedServic__ in *IServiceCollection* and register and configure your created job.
    ``` C#
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
    ```