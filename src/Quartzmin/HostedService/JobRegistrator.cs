using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkierQuartz.HostedService
{
    internal class JobRegistrator : IJobRegistrator
    {
        public JobRegistrator(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
