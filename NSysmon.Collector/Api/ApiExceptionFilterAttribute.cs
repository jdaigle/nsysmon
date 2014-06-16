using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace NSysmon.Collector.Api
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public static log4net.ILog Log = log4net.LogManager.GetLogger("NSysmon.Collector.Api");

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Log.Error("Error Executing API Action", actionExecutedContext.Exception);
            base.OnException(actionExecutedContext);
        }

        public override Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, System.Threading.CancellationToken cancellationToken)
        {
            Log.Error("Error Executing API Action", actionExecutedContext.Exception);
            return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
        }
    }
}
