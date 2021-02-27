using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace SilkierQuartz.Middlewares
{
    /// <summary>
    /// Middleware that performs authentication.
    /// </summary>
    public class SilkierQuartzAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of <see cref="Microsoft.AspNetCore.Authentication.AuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next item in the middleware pipeline.</param>
        /// <param name="schemes">The <see cref="IAuthenticationSchemeProvider"/>.</param>
        public SilkierQuartzAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
        }

        /// <summary>
        /// Gets or sets the <see cref="IAuthenticationSchemeProvider"/>.
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; set; }

        /// <summary>
        /// Invokes the middleware performing authentication.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        public async Task Invoke(HttpContext context)
        {
            var relativePath = GetRelativeUrlPath(context);
            if (relativePath.StartsWith(SilkierQuartzAuthenticateConfig.VirtualPathRoot) ||
                relativePath.StartsWith("?ReturnUrl") &&
                relativePath.Contains(SilkierQuartzAuthenticateConfig.VirtualPathRootUrlEncode))
            {
                await DetailProcess(context, SilkierQuartzAuthenticateConfig.AuthScheme);
            }
            
            await _next(context);
        }

        public async Task DetailProcess(HttpContext httpContext, string authSchemeName = null)
        {
            httpContext.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            {
                OriginalPath = httpContext.Request.Path,
                OriginalPathBase = httpContext.Request.PathBase
            });

            // Give any IAuthenticationRequestHandler schemes a chance to handle the request
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            {
                if (await handlers.GetHandlerAsync(httpContext, scheme.Name) is IAuthenticationRequestHandler handler &&
                    await handler.HandleRequestAsync())
                {
                    return;
                }
            }

            var authScheme = string.IsNullOrEmpty(authSchemeName)
                ? await Schemes.GetDefaultAuthenticateSchemeAsync()
                : await Schemes.GetSchemeAsync(authSchemeName);
            if (authScheme != null)
            {
                var result = await httpContext.AuthenticateAsync(authScheme.Name);
                if (result.Principal == null || !result.Principal.HasClaim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim,
                    SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaimValue))
                {
                    return;
                }
                if (result?.Principal != null)
                {
                    httpContext.User = result.Principal;
                }
            }
        }

        public string GetRelativeUrlPath(HttpContext httpContext)
        {
            /*
                In some cases, like when running integration tests with WebApplicationFactory<T>
                the RawTarget returns an empty string instead of null, in that case we can't use
                ?? as fallback.
            */
            if (httpContext == null)
            {
                return string.Empty;
            }

            var requestPath = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = httpContext.Request.Path.ToString();
            }

            return requestPath;
        }
    }
}