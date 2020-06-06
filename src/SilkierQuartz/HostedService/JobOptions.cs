using System;
using System.Collections.Generic;
using System.Text;

namespace SilkierQuartz.HostedService
{
    public class JobOptions
    {
        public TriggerOptions[] Triggers { get; set; }
    }
}
