using Otc.RequestTracking.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class OtcRequestTrackingApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestTracking(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(applicationBuilder));
            }

            applicationBuilder.UseMiddleware<RequestTrackerMiddleware>();

            return applicationBuilder;
        }
    }
}
