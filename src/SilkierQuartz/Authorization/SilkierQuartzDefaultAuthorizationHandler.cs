using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace SilkierQuartz.Authorization
{
    public class SilkierQuartzDefaultAuthorizationHandler : AuthorizationHandler<SilkierQuartzDefaultAuthorizationRequirement>
    {
        private readonly SilkierQuartzAuthenticationOptions options;

        public SilkierQuartzDefaultAuthorizationHandler(SilkierQuartzAuthenticationOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SilkierQuartzDefaultAuthorizationRequirement requirement)
        {
            if (context.HasSucceeded || options.SkipDefaultRequirementHandler)
            {
                return Task.CompletedTask;
            }

            if (options.AccessRequirement == SilkierQuartzAuthenticationOptions.SimpleAccessRequirement.AllowAnonymous)
            {
                context.Succeed(requirement);

                return Task.CompletedTask;
            }

            if (!context.User.Identity.IsAuthenticated &&
                options.AccessRequirement == SilkierQuartzAuthenticationOptions.SimpleAccessRequirement.AllowOnlyAuthenticated)
            {
                context.Fail();

                return Task.CompletedTask;
            }

            if (!context.User.HasClaim(options.SilkierQuartzClaim, options.SilkierQuartzClaimValue) &&
                options.AccessRequirement == SilkierQuartzAuthenticationOptions.SimpleAccessRequirement.AllowOnlyUsersWithClaim)
            {
                context.Fail();

                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
