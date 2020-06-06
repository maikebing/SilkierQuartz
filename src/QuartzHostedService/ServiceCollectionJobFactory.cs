using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace QuartzHostedService
{
    public class ServiceCollectionJobFactory : IJobFactory
    {
        protected readonly IServiceProvider Container;
        private ConcurrentDictionary<IJob, IServiceScope> _createdJob = new ConcurrentDictionary<IJob, IServiceScope>();

        public ServiceCollectionJobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scoped = Container.CreateScope();
            var result = scoped.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            _createdJob.AddOrUpdate(result, scoped, (j, s) => scoped);
            return result;
        }

        public void ReturnJob(IJob job)
        {
            if (_createdJob.TryRemove(job, out var scope))
            {
                scope.Dispose();
            }

            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}