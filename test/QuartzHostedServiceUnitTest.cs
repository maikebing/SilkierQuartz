using FakeItEasy;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SilkierQuartz.HostedService;
namespace SilkierQuartz.Test
{
    public class QuartzHostedServiceUnitTest
    {
        [Fact(DisplayName = "Install JobFactory (For DI)")]
        public async void IServiceCollectionExtensions_Register_HostedService()
        {
            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IEnumerable<IScheduleJob>))).Returns(null);

            ISchedulerFactory schedulerFactory = A.Fake<ISchedulerFactory>();
            IScheduler scheduler = A.Fake<IScheduler>();
            A.CallTo(() => schedulerFactory.GetScheduler(A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(scheduler));

            IJobFactory jobFactory = A.Fake<IJobFactory>();

            var testClass = new QuartzHostedService(serviceProvider, schedulerFactory, jobFactory);
            await testClass.StartAsync(CancellationToken.None);

            A.CallTo(scheduler)
                .Where(a => a.Method.Name.Equals("set_JobFactory"));
        }


        [Fact(DisplayName = "All Registered Job")]
        public async void IServiceCollectionExtensions_Register_RegisterJob()
        {
            // Registered jobs
            var scheduleJobc = new List<IScheduleJob>();
            var jobDetail1 = A.Fake<IJobDetail>();
            var jobDetail2 = A.Fake<IJobDetail>();
            var jobDetail3 = A.Fake<IJobDetail>();

            scheduleJobc.Add(new ScheduleJob(jobDetail1, new List<ITrigger>() { A.Fake<ITrigger>() }));
            scheduleJobc.Add(new ScheduleJob(jobDetail2, new List<ITrigger>() { A.Fake<ITrigger>() }));
            scheduleJobc.Add(new ScheduleJob(jobDetail3, new List<ITrigger>() { A.Fake<ITrigger>() }));

            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IEnumerable<IScheduleJob>))).Returns(scheduleJobc);


            ISchedulerFactory schedulerFactory = A.Fake<ISchedulerFactory>();
            IScheduler scheduler = A.Fake<IScheduler>();
            A.CallTo(() => schedulerFactory.GetScheduler(A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(scheduler));

            IJobFactory jobFactory = A.Fake<IJobFactory>();

            var testClass = new QuartzHostedService(serviceProvider, schedulerFactory, jobFactory);
            await testClass.StartAsync(CancellationToken.None);

            A.CallTo(
                () => scheduler.ScheduleJob(
                    A<IJobDetail>.That.Matches(jd => jd == jobDetail1 || jd == jobDetail2 || jd == jobDetail3),
                    A<ITrigger>.Ignored,
                    A<CancellationToken>.Ignored))
                .MustHaveHappened(3, Times.Exactly);
        }

        [Fact(DisplayName = "Started all registered Job with specified ITrigger")]
        public async void IServiceCollectionExtensions_Register_RegisterJob_ITrigger()
        {
            // Registered jobas
            var scheduleJobc = new List<IScheduleJob>();
            var jobDetail1 = A.Fake<IJobDetail>();

            var trigger1 = A.Fake<ITrigger>();
            var trigger2 = A.Fake<ITrigger>();
            var trigger3 = A.Fake<ITrigger>();
            scheduleJobc.Add(new ScheduleJob(jobDetail1, new List<ITrigger>() { trigger1, trigger2, trigger3 }));

            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IEnumerable<IScheduleJob>))).Returns(scheduleJobc);


            ISchedulerFactory schedulerFactory = A.Fake<ISchedulerFactory>();
            IScheduler scheduler = A.Fake<IScheduler>();
            A.CallTo(() => schedulerFactory.GetScheduler(A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(scheduler));

            IJobFactory jobFactory = A.Fake<IJobFactory>();

            var testClass = new QuartzHostedService(serviceProvider, schedulerFactory, jobFactory);
            await testClass.StartAsync(CancellationToken.None);

            A.CallTo(
                () => scheduler.ScheduleJob(
                    A<IJobDetail>.That.Matches(jd => jd == jobDetail1),
                    A<ITrigger>.That.Matches(t => t == trigger1),
                    A<CancellationToken>.Ignored))
                .MustHaveHappened(1,Times.Exactly);

            A.CallTo(
                () => scheduler.ScheduleJob(
                    A<ITrigger>.That.Matches(t => t == trigger2 || t == trigger3),
                    A<CancellationToken>.Ignored))
                .MustHaveHappened(2,Times.Exactly);
        }
    }
}
