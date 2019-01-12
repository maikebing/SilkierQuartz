using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreSampleQuartzHostedService
{
    public class AppSettings
    {
        public bool EnableHelloJob { get; set; }
        public bool EnableHelloSingleJob { get; set; }
    }
}
