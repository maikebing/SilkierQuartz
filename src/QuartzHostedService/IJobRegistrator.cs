using Microsoft.Extensions.DependencyInjection;

namespace QuartzHostedService
{
    public interface IJobRegistrator
    {
        IServiceCollection Services { get; }
    }
}