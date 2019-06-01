using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzHostedService
{
    public static class IJobRegistratorExtensions
    {
        public static IJobRegistrator RegiserCRONJob<TJob>(
            this IJobRegistrator jobRegistrator,
            Action<JobOptions> jobOptions)
            where TJob : class, IJob
        {
            var options = new JobOptions();
            jobOptions?.Invoke(options);

            return jobRegistrator.RegiserJob<TJob>(
                options.Triggers?.Select(n => n.CreateTriggerBuilder())
                );
        }


        public static IJobRegistrator RegiserJob<TJob>(
            this IJobRegistrator jobRegistrator,
            Func<IEnumerable<TriggerBuilder>> triggers)
            where TJob : class, IJob
        {

            return jobRegistrator.RegiserJob<TJob>(triggers());
        }
        public static IServiceCollection AddQuartzJobDetail(this IServiceCollection services, Func<IJobDetail> detail)
        {
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(detail(), new List<ITrigger>()));
            return services;
        }
        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services, string identity) where TJob : class
        {
            return services.AddQuartzJob<TJob>(identity, null);
        }

        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services, string identity, string description) where TJob : class
        {
            if (!services.Any(sd => sd.ServiceType == typeof(TJob)))
            {
                services.AddTransient<TJob>();
            }
            var jobDetail = JobBuilder.Create(typeof(TJob)).WithIdentity(identity).WithDescription(description).Build();
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, new List<ITrigger>()));
            return services;
        }


        public static IServiceCollection AddQuartzJobDetail(this IServiceCollection services, IJobDetail detail) 
        {
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(detail, new List<ITrigger>()));
            return services;
        }
        public static IServiceCollection AddQuartzJob<TJob>(this IServiceCollection services) where TJob : class
        {
            services.AddTransient<TJob>();
            var jobDetail = JobBuilder.Create(typeof(TJob)).Build();
            services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, new List<ITrigger>()));
            return services;
        }
        public static IApplicationBuilder UseQuartzJob<TJob>(
                this IApplicationBuilder app,
                Func<TriggerBuilder> triggerBuilder_func)
                where TJob : class, IJob
        {
            return app.UseQuartzJob<TJob>(new TriggerBuilder[] { triggerBuilder_func() });
        }
        public static IApplicationBuilder UseQuartzJob<TJob>(
            this IApplicationBuilder app,string JobKey,
            Func<TriggerBuilder> triggerBuilder_func)
            where TJob : class, IJob
        {
            var _scheduleJobs = app.ApplicationServices.GetService<IEnumerable<IScheduleJob>>();
            var job = from js in _scheduleJobs where js.JobDetail.JobType == typeof(TJob) && js.JobDetail.Key.Name == JobKey select js;
            if (job.Any())
            {
                var scheduleJob = job.First();
                var lstgs = (List<ITrigger>)scheduleJob.Triggers;
                lstgs.Add(triggerBuilder_func().ForJob(scheduleJob.JobDetail).Build());
            }
            return app;
        }

        public static IApplicationBuilder UseQuartzJob<TJob>(
                this IApplicationBuilder app,
                TriggerBuilder triggerBuilder)
                where TJob : class, IJob
        {
            return app.UseQuartzJob<TJob>(new TriggerBuilder[] { triggerBuilder });
        }

        public static IApplicationBuilder UseQuartzJob<TJob>(
            this IApplicationBuilder app,
            Func<IEnumerable<TriggerBuilder>> triggerBuilders_func)
            where TJob : class, IJob
        {
            return app.UseQuartzJob<TJob>(triggerBuilders_func());
        }
        public static IApplicationBuilder UseQuartzJob<TJob>(
          this IApplicationBuilder app,
          IEnumerable<TriggerBuilder> triggerBuilders)
          where TJob : class, IJob
        {
            var _scheduleJobs = app.ApplicationServices.GetService<IEnumerable<IScheduleJob>>();
            var job = from js in _scheduleJobs where js.JobDetail.JobType == typeof(TJob) select js;
            if (job.Any())
            {
                var scheduleJob = job.First();
                var lstgs = (List<ITrigger>)scheduleJob.Triggers;
                foreach (var triggerBuilder in triggerBuilders)
                {
                    lstgs.Add(triggerBuilder.ForJob(scheduleJob.JobDetail).Build());
                }
            }
            return app;
        }
        public static IJobRegistrator RegiserJob<TJob>(
            this IJobRegistrator jobRegistrator,
            IEnumerable<TriggerBuilder> triggerBuilders)
            where TJob : class, IJob
        {
            jobRegistrator.Services.AddTransient<TJob>();

            var jobDetail = JobBuilder.Create<TJob>().Build();

            List<ITrigger> triggers = new List<ITrigger>(triggerBuilders.Count());
            foreach (var triggerBuilder in triggerBuilders)
            {
                triggers.Add(triggerBuilder.ForJob(jobDetail).Build());
            }

            jobRegistrator.Services.AddSingleton<IScheduleJob>(provider => new ScheduleJob(jobDetail, triggers));

            return jobRegistrator;
        }
    }
}
