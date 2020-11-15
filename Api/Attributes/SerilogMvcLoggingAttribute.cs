using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SerilogMvcLoggingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Serilog does not include these by default but they are useful to add https://nblumhardt.com/2019/10/serilog-mvc-logging/
            //
            IDiagnosticContext diagnosticContext = context.HttpContext.RequestServices.GetService<IDiagnosticContext>();
            diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
        }
    }
}