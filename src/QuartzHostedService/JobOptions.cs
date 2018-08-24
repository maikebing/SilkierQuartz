using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzHostedService
{
    public class JobOptions
    {
        public TriggerOptions[] Triggers { get; set; }
    }
}
