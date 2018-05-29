using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Otc.RequestTracking.AspNetCore
{
    public class RequestTrackerMiddleware
    {
        private readonly RequestDelegate next;

        public RequestTrackerMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext, RequestTracker requestTracker)
        {
            requestTracker.LogRequestIfShouldLogIt(httpContext.Request);
            await next.Invoke(httpContext);
        }
    }
}
