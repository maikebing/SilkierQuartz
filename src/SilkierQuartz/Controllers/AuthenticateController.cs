using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilkierQuartz.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SilkierQuartz.Controllers
{
    [AllowAnonymous]
    public class AuthenticateController : PageControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Login([FromServices] IAuthenticationSchemeProvider schemes)
        {
            var silkierScheme = await schemes.GetSchemeAsync(SilkierQuartzAuthenticateConfig.AuthScheme);

            if (string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserName) ||
                string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserPassword))
            {
                foreach (var userClaim in HttpContext.User.Claims)
                {
                    Debug.WriteLine($"{userClaim.Type} - {userClaim.Value}");
                }

                if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated ||
                    !HttpContext.User.HasClaim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim,
                        SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaimValue))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserName) ? "SilkierQuartzAdmin" : SilkierQuartzAuthenticateConfig.UserName ),
                        new Claim(ClaimTypes.Name, string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserPassword) ? "SilkierQuartzPassword" : SilkierQuartzAuthenticateConfig.UserPassword),
                        new Claim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim, SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaimValue)
                    };

                    var authProperties = new AuthenticationProperties()
                    {
                        IsPersistent = SilkierQuartzAuthenticateConfig.IsPersist
                    };

                    var userIdentity = new ClaimsIdentity(claims, SilkierQuartzAuthenticateConfig.AuthScheme);
                    await HttpContext.SignInAsync(SilkierQuartzAuthenticateConfig.AuthScheme, new ClaimsPrincipal(userIdentity),
                        authProperties);

                    return RedirectToAction(nameof(SchedulerController.Index), nameof(Scheduler));
                }
                else
                {
                    return RedirectToAction(nameof(SchedulerController.Index), nameof(Scheduler));
                }
            }
            else
            {
                if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated ||
                    !HttpContext.User.HasClaim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim, "Authorized"))
                {
                    ViewBag.IsLoginError = false;
                    return View(new AuthenticateViewModel());
                }
                else
                {
                    return RedirectToAction(nameof(SchedulerController.Index), nameof(Scheduler));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthenticateViewModel request)
        {
            if (string.Compare(request.UserName, SilkierQuartzAuthenticateConfig.UserName,
                StringComparison.InvariantCulture) != 0 || 
                string.Compare(request.Password, SilkierQuartzAuthenticateConfig.UserPassword, 
                    StringComparison.InvariantCulture) != 0)
            {
                request.IsLoginError = true;
                return View(request);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserName) ? "SilkierQuartzAdmin" : SilkierQuartzAuthenticateConfig.UserName ),
                new Claim(ClaimTypes.Name, string.IsNullOrEmpty(SilkierQuartzAuthenticateConfig.UserPassword) ? "SilkierQuartzPassword" : SilkierQuartzAuthenticateConfig.UserPassword),
                new Claim(SilkierQuartzAuthenticateConfig.SilkierQuartzSpecificClaim, "Authorized")
            };

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = request.IsPersist
            };

            var userIdentity = new ClaimsIdentity(claims, SilkierQuartzAuthenticateConfig.AuthScheme);
            await HttpContext.SignInAsync(SilkierQuartzAuthenticateConfig.AuthScheme, new ClaimsPrincipal(userIdentity),
                authProperties);

            return RedirectToAction(nameof(SchedulerController.Index), nameof(Scheduler));
        }

        [HttpGet]
        [Authorize(SilkierQuartzAuthenticateConfig.AuthScheme)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(SilkierQuartzAuthenticateConfig.AuthScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
