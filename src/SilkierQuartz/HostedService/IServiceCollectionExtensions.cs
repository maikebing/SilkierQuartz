using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SilkierQuartz.HostedService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
namespace SilkierQuartz
{
    public static class IServiceCollectionExtensions
    {
        public static IJobRegistrator UseQuartzHostedService(this IServiceCollection services)
        {
            return UseQuartzHostedService(services, null);
        }
        public static IServiceCollection AddQuartzHostedService(this IServiceCollection services)
        {
            return AddQuartzHostedService(services, null);

        }
        public static IServiceCollection AddQuartzHostedService(this IServiceCollection services,
                                                    Action<NameValueCollection> stdSchedulerFactoryOptions)
        {
            var re = services.UseQuartzHostedService(stdSchedulerFactoryOptions);
            return re.Services;
        }
    
        private static bool _quartzHostedServiceIsAdded = false;
        /// <summary>
        ///  Must be call after Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults()
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureQuartzHost(this IHostBuilder builder)
        {
            _quartzHostedServiceIsAdded = true;
            return builder.ConfigureServices(services => services.AddHostedService<QuartzHostedService>());
        }


        public static IJobRegistrator UseQuartzHostedService(this IServiceCollection services,
        Action<NameValueCollection> stdSchedulerFactoryOptions)
        {
            if (!_quartzHostedServiceIsAdded)
            {
                services.AddHostedService<QuartzHostedService>();
            }
            services.AddTransient<ISchedulerFactory>(provider =>
            {
                var options = new NameValueCollection();
                stdSchedulerFactoryOptions?.Invoke(options);
                var result = new StdSchedulerFactory();
                if (options.Count > 0)
                    result.Initialize(options);
                return result;
            });
            services.AddSingleton<IJobFactory, ServiceCollectionJobFactory>();
            return new JobRegistrator(services);
        }
    }
}
