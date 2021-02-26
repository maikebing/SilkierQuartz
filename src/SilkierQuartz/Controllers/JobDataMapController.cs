using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilkierQuartz.Helpers;
using SilkierQuartz.TypeHandlers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkierQuartz.Controllers
{
    [Authorize(SilkierQuartzAuthenticateConfig.AuthScheme)]
    public class JobDataMapController : PageControllerBase
    {
        [HttpPost, JsonErrorResponse]
        public async Task<IActionResult> ChangeType()
        {
            var formData = await Request.GetFormData();

            TypeHandlerBase selectedType, targetType;
            try
            {
                selectedType = Services.TypeHandlers.Deserialize((string)formData.First(x => x.Key == "selected-type").Value);
                targetType = Services.TypeHandlers.Deserialize((string)formData.First(x => x.Key == "target-type").Value);
            }
            catch (JsonSerializationException ex) when (ex.Message.StartsWith("Could not create an instance of type"))
            {
                return new BadRequestResult();
            }

            var dataMapForm = (await formData.GetJobDataMapForm(includeRowIndex: false)).SingleOrDefault(); // expected single row

            object oldValue = selectedType.ConvertFrom(dataMapForm);

            // phase 1: direct conversion
            object newValue = targetType.ConvertFrom(oldValue);

            if (oldValue != null && newValue == null) // if phase 1 failed
            {
                // phase 2: conversion using invariant string
                var str = selectedType.ConvertToString(oldValue);
                newValue = targetType.ConvertFrom(str);
            }

            return Html(targetType.RenderView(Services, newValue));
        }

        [HttpGet, ActionName("TypeHandlers.js")]
        public IActionResult TypeHandlersScript()
        {
            var etag = Services.TypeHandlers.LastModified.ETag();

            if (etag.Equals(GetETag()))
                return NotModified();

            StringBuilder execStubBuilder = new StringBuilder();
            execStubBuilder.AppendLine();
            foreach (var func in new[] { "init" })
                execStubBuilder.AppendLine(string.Format("if (f === '{0}' && {0} !== 'undefined') {{ {0}.call(this); }}", func));

            string execStub = execStubBuilder.ToString();

            var js = Services.TypeHandlers.GetScripts().ToDictionary(x => x.Key,
                x => new JRaw("function(f) {" + x.Value + execStub + "}"));

            return TextFile("var $typeHandlerScripts = " + JsonConvert.SerializeObject(js) + ";",
                "application/javascript", Services.TypeHandlers.LastModified, etag);
        }
    }
}
