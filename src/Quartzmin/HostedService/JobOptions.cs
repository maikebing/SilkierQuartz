using System;
using System.Collections.Generic;
using System.Text;

namespace Quartzmin.HostedService
{
    public class JobOptions
    {
        public TriggerOptions[] Triggers { get; set; }
    }
}
