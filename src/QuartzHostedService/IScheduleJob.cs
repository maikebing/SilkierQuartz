using System.Collections.Generic;
using Quartz;

namespace QuartzHostedService
{
    internal interface IScheduleJob
    {
        IJobDetail JobDetail { get; set; }
        IEnumerable<ITrigger> Triggers { get; set; }
    }
}