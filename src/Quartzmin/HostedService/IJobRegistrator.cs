using Microsoft.Extensions.DependencyInjection;

namespace Quartzmin.HostedService
{
    public interface IJobRegistrator
    {
        IServiceCollection Services { get; }
    }
}