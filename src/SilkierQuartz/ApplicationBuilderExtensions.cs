
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using Quartz.Impl;
using SilkierQuartz.Middlewares;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace SilkierQuartz
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///  Returns a client-usable handle to a Quartz.IScheduler.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IScheduler GetScheduler(this IApplicationBuilder app)
        {
            return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler().Result;
        }
        /// <summary>
        ///  Returns a handle to the Scheduler with the given name, if it exists.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="schedName"></param>
        /// <returns></returns>
        public static IScheduler GetScheduler(this IApplicationBuilder app, string schedName)
        {
            return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler(schedName).Result;
        }
        /// <summary>
        /// Returns handles to all known Schedulers (made by any SchedulerFactory within  this app domain.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IReadOnlyList<IScheduler> GetAllSchedulers(this IApplicationBuilder app)
        {
            return app.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetAllSchedulers().Result;
        }

        [Obsolete("We recommend UseSilkierQuartz")]
        public static IApplicationBuilder UseQuartzmin(this IApplicationBuilder app, SilkierQuartzOptions options, Action<Services> configure = null)
            => app.UseSilkierQuartz(options, configure);

        /// <summary>
        /// Use SilkierQuartz and automatically discover IJob subclasses with SilkierQuartzAttribute
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <param name="configure"></param>
        public static IApplicationBuilder UseSilkierQuartz(this IApplicationBuilder app, SilkierQuartzOptions options, Action<Services> configure = null)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            app.UseFileServer(options);
            if (options.Scheduler == null)
            {
                try
                {
                    options.Scheduler = app.ApplicationServices.GetRequiredService<ISchedulerFactory>()?.GetScheduler().Result;
                }
                catch (Exception)
                {
                    options.Scheduler = null;
                }
                if (options.Scheduler == null)
                {
                    options.Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
                }
            }
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

               SilkierQuartzAuthenticateConfig.VirtualPathRoot = options.VirtualPathRoot;
               SilkierQuartzAuthenticateConfig.VirtualPathRootUrlEncode = options.VirtualPathRoot.Replace("/", "%2F");
               SilkierQuartzAuthenticateConfig.UserName = options.AccountName;
               SilkierQuartzAuthenticateConfig.UserPassword = options.AccountPassword;
               SilkierQuartzAuthenticateConfig.IsPersist = options.IsAuthenticationPersist;
               endpoints.MapControllerRoute($"{nameof(SilkierQuartz)}Authenticate",
                   $"{options.VirtualPathRoot}{{controller=Authenticate}}/{{action=Login}}");
           });

            var types = GetSilkierQuartzJobs();
            types.ForEach(t =>
            {
                var so = t.GetCustomAttribute<SilkierQuartzAttribute>();
                app.UseQuartzJob(t, () =>
                {
                    var tb = TriggerBuilder.Create();
                    tb.WithSimpleSchedule(x =>
                    {
                        x.WithInterval(so.WithInterval);
                        if (so.RepeatCount > 0)
                        {
                            x.WithRepeatCount(so.RepeatCount);

                        }
                        else
                        {
                            x.RepeatForever();
                        }
                    });
                    if (so.StartAt == DateTimeOffset.MinValue)
                    {
                        tb.StartNow();
                    }
                    else
                    {
                        tb.StartAt(so.StartAt);
                    }
                    var tk = new TriggerKey(!string.IsNullOrEmpty(so.TriggerName) ? so.TriggerName : $"{t.Name}'s Trigger");
                    if (!string.IsNullOrEmpty(so.TriggerGroup))
                    {
                        so.TriggerGroup = so.TriggerGroup;
                    }
                    tb.WithIdentity(tk);
                    tb.WithDescription(so.TriggerDescription ?? $"{t.Name}'s Trigger,full name is {t.FullName}");
                    if (so.Priority > 0) tb.WithPriority(so.Priority);
                    return tb;
                });

            });


            return app;
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
        [Obsolete("We recommend AddSilkierQuartz")]
        public static IServiceCollection AddQuartzmin(this IServiceCollection services, Action<NameValueCollection> stdSchedulerFactoryOptions = null)
            => services.AddSilkierQuartz(stdSchedulerFactoryOptions);


        public static IServiceCollection AddSilkierQuartz(this IServiceCollection services, Action<NameValueCollection> stdSchedulerFactoryOptions = null, Func<List<Assembly>> jobsasmlist = null)
        {
            services.AddControllers()
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddNewtonsoftJson();

            services.AddAuthentication(SilkierQuartzAuthenticateConfig.AuthScheme).AddCookie(
                SilkierQuartzAuthenticateConfig.AuthScheme,
                cfg =>
                {
                    cfg.Cookie.Name = SilkierQuartzAuthenticateConfig.AuthScheme;
                    cfg.LoginPath = $"{SilkierQuartzAuthenticateConfig.VirtualPathRoot}/Authenticate/Login";
                    cfg.AccessDeniedPath = $"{SilkierQuartzAuthenticateConfig.VirtualPathRoot}/Authenticate/Login";
                    if (SilkierQuartzAuthenticateConfig.IsPersist)
                    {
                        cfg.ExpireTimeSpan = TimeSpan.FromDays(7);
                        cfg.SlidingExpiration = true;
                    }
                });

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(SilkierQuartzAuthenticateConfig.AuthScheme, authBuilder =>
                {
                    authBuilder.RequireAuthenticatedUser();
                    authBuilder.RequireClaim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim,
                        SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaimValue);
                });
            });

            services.UseQuartzHostedService(stdSchedulerFactoryOptions);

            var types = GetSilkierQuartzJobs(jobsasmlist?.Invoke());
            types.ForEach(t =>
            {
                var so = t.GetCustomAttribute<SilkierQuartzAttribute>();
                services.AddQuartzJob(t, so.Identity ?? t.Name, so.Desciption ?? t.FullName);
            });
            return services;
        }
        private static List<Type> _silkierQuartzJobs = null;
        private static List<Type> GetSilkierQuartzJobs(List<Assembly> lists = null)
        {
            if (_silkierQuartzJobs == null)
            {
                try
                {
                    var types1 = from t in Assembly.GetEntryAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                    var types = from t in Assembly.GetCallingAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                    _silkierQuartzJobs = new List<Type>();
                    _silkierQuartzJobs.AddRange(types.ToList());
                    _silkierQuartzJobs.AddRange(types1.ToList());
                    lists?.ForEach(asm =>
                    {
                        var typeasm = from t in asm.GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                        _silkierQuartzJobs.AddRange(typeasm);
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception("Can't  find  type with  IJob and have  SilkierQuartzAttribute", ex);
                }
            }
            return _silkierQuartzJobs;
        }

        /// <summary>
        /// Adds the <see cref="SilkierQuartzAuthenticationMiddleware"/> to the specified <see cref="IApplicationBuilder"/>, which enables simple authentication for silkier Quartz.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder AddSilkierQuartzAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<SilkierQuartzAuthenticationMiddleware>();
        }
    }
}

