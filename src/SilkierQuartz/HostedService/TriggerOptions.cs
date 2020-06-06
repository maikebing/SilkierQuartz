using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkierQuartz.HostedService
{
    public class TriggerOptions
    {
        public string CRONExpression { get; set; }

        public int? Priority { get; set; }

        public virtual TriggerBuilder CreateTriggerBuilder()
        {
            var result = TriggerBuilder.Create();
            if (Priority.HasValue)
                result.WithPriority(Priority.Value);

            if(!string.IsNullOrEmpty(CRONExpression))
                result.WithCronSchedule(CRONExpression);
            
            return result;
        }
    }
}
