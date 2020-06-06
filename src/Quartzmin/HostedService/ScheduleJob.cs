using Quartz;
using System;
using System.Collections.Generic;

namespace Quartzmin
{
    internal class ScheduleJob : IScheduleJob
    {
        public ScheduleJob(IJobDetail jobDetail, IEnumerable<ITrigger> triggers)
        {
            JobDetail = jobDetail;
            Triggers = triggers;
        }

        public IJobDetail JobDetail { get; set; }
        public IEnumerable<ITrigger> Triggers { get; set; }
    }
}
