using Microsoft.Extensions.DependencyInjection;

namespace SilkierQuartz.HostedService
{
    public interface IJobRegistrator
    {
        IServiceCollection Services { get; }
    }
}