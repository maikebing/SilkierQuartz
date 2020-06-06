using Microsoft.Extensions.DependencyInjection;

namespace Quartzmin
{
    public interface IJobRegistrator
    {
        IServiceCollection Services { get; }
    }
}