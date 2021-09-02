using Microsoft.AspNetCore.Authorization;
using static SilkierQuartz.SilkierQuartzAuthenticationOptions;

namespace SilkierQuartz.Authorization
{
    public class SilkierQuartzDefaultAuthorizationRequirement : IAuthorizationRequirement
    {
        public SimpleAccessRequirement AccessRequirement { get; }

        public SilkierQuartzDefaultAuthorizationRequirement(SimpleAccessRequirement accessRequirement)
        {
            AccessRequirement = accessRequirement;
        }
    }
}
