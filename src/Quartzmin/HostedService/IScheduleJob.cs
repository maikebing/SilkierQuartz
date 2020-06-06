using System.Collections.Generic;
using Quartz;

namespace Quartzmin
{
    internal interface IScheduleJob
    {
        IJobDetail JobDetail { get; set; }
        IEnumerable<ITrigger> Triggers { get; set; }
    }
}