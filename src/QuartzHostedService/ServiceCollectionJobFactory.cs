using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzHostedService
{
    public class ServiceCollectionJobFactory : IJobFactory
    {
        protected readonly IServiceProvider Container;

        public ServiceCollectionJobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            using (var scoped = Container.CreateScope())
            {
                return scoped.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            }
	}

        public void ReturnJob(IJob job)
        {

        }
    }
}