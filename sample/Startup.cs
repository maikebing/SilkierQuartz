using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using SilkierQuartz.Example.Jobs;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace SilkierQuartz.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            //services.AddSilkierQuartz(serviceCfg =>
            //    serviceCfg.AddControllers()
            //        .AddApplicationPart(Assembly.GetExecutingAssembly()));

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            var sectionData = Configuration.GetSection("Quartz");
            var dictionaryData = sectionData.GetChildren().ToDictionary(x => x.Key, x => x.Value);
            var quartzConfig = new NameValueCollection();
            foreach (var kvp in dictionaryData)
            {
                string value = null;
                if (kvp.Value != null)
                {
                    value = kvp.Value;
                }
                quartzConfig.Add(kvp.Key, value);
            }

            services.AddQuartz(quartzConfig, q =>
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                q.UseSimpleTypeLoader();
            });
            services.AddScoped<DemoScheduler.DummyJob>();

            services.AddOptions();
            services.Configure<AppSettings>(Configuration);
            services.Configure<InjectProperty>(options => { options.WriteText = "This is inject string"; });
            services.AddQuartzJob<HelloJob>()
                    .AddQuartzJob<InjectSampleJob>()
                    .AddQuartzJob<HelloJobSingle>()
                    .AddQuartzJob<InjectSampleJobSingle>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            var silkierQuartzOptions = new SilkierQuartzOptions()
            {
                VirtualPathRoot = "/SilkierQuartz",
                UseLocalTime = true,
                DefaultDateFormat = "yyyy-MM-dd",
                DefaultTimeFormat = "HH:mm:ss",
                CronExpressionOptions = new CronExpressionDescriptor.Options()
                {
                    DayOfWeekStartIndexZero = false //Quartz uses 1-7 as the range
                }
            };
            app.UseSilkierQuartz(silkierQuartzOptions,
                appBuilder => appBuilder.UseEndpoints(endpoints => endpoints.MapControllerRoute(nameof(SilkierQuartz),
                    $"{silkierQuartzOptions.VirtualPathRoot}/{{controller=Scheduler}}/{{action=Index}}")));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            //How to compatible old code to SilkierQuartz
            //将旧的原来的规划Job的代码进行移植兼容的示例
            //Remove this line cause the job was saved
            app.SchedulerJobs();


            #region  不使用 SilkierQuartzAttribe 属性的进行注册和使用的IJob，这里通过UseQuartzJob的IJob必须在  ConfigureServices进行AddQuartzJob

            app.UseQuartzJob<HelloJobSingle>(TriggerBuilder.Create().WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()))
            .UseQuartzJob<InjectSampleJobSingle>(() =>
            {
                return TriggerBuilder.Create()
                   .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever());
            });

            app.UseQuartzJob<HelloJob>(new List<TriggerBuilder>
                {
                    TriggerBuilder.Create()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()),
                    TriggerBuilder.Create()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(2).RepeatForever()),
                     //Add a sample that uses 1-7 for dow
                    TriggerBuilder.Create()
                                  .WithCronSchedule("0 0 2 ? * 7 *"),
                });

            app.UseQuartzJob<InjectSampleJob>(() =>
            {
                var result = new List<TriggerBuilder>();
                result.Add(TriggerBuilder.Create()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
                return result;
            });
            #endregion
        }
    }
}
