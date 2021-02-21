
namespace SilkierQuartz.Helpers
{
#if (NETSTANDARD || NETCOREAPP)
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Text.Json;

    public class JsonErrorResponseAttribute : ActionFilterAttribute
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver(), // PascalCase as default
        };
        private static readonly object _systemTextSerializerOptions = new JsonSerializerOptions();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<JsonResult>>();
                var serializerOptions = executor.GetType().FullName.Contains("SystemTextJson")
                    ? _systemTextSerializerOptions
                    : _serializerSettings;

                context.Result = new JsonResult(new { ExceptionMessage = context.Exception.Message }, serializerOptions) { StatusCode = 400 };
                context.ExceptionHandled = true;
            }
        }
    }
#endif

#if NETFRAMEWORK
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http.Filters;
    using Newtonsoft.Json;

    public class JsonErrorResponseAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            if (actionContext.Exception != null)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { ExceptionMessage = actionContext.Exception.GetBaseException().Message }), Encoding.UTF8, "application/json")
                };
            }
        }
    }
#endif

}
