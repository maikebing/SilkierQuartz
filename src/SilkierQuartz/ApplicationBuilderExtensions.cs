
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SilkierQuartz;
using System;
using System.Collections.Specialized;
using System.Reflection;

namespace SilkierQuartz
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkierQuartz(this IApplicationBuilder app, SilkierQuartzOptions options, Action<Services> configure = null)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            app.UseFileServer(options);

            var services = Services.Create(options);
            configure?.Invoke(services);

            app.Use(async (context, next) =>
            {
                context.Items[typeof(Services)] = services;
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
           {
               endpoints.MapControllerRoute(nameof(SilkierQuartz), $"{options.VirtualPathRoot}/{{controller=Scheduler}}/{{action=Index}}");
           });

        }

        private static void UseFileServer(this IApplicationBuilder app, SilkierQuartzOptions options)
        {
            IFileProvider fs;
            if (string.IsNullOrEmpty(options.ContentRootDirectory))
                fs = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Content");
            else
                fs = new PhysicalFileProvider(options.ContentRootDirectory);

            var fsOptions = new FileServerOptions()
            {
                RequestPath = new PathString($"{options.VirtualPathRoot}/Content"),
                EnableDefaultFiles = false,
                EnableDirectoryBrowsing = false,
                FileProvider = fs
            };

            app.UseFileServer(fsOptions);
        }

        public static void AddSilkierQuartz(this IServiceCollection services, Action<NameValueCollection> stdSchedulerFactoryOptions = null)
        {
            services.AddControllers()
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddNewtonsoftJson();
            services.UseQuartzHostedService(stdSchedulerFactoryOptions);

        }


    }
}

