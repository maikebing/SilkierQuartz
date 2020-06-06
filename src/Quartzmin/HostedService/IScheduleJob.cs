using System.Collections.Generic;
using Quartz;

namespace SilkierQuartz.HostedService
{
    internal interface IScheduleJob
    {
        IJobDetail JobDetail { get; set; }
        IEnumerable<ITrigger> Triggers { get; set; }
    }
}