using Microsoft.Extensions.DependencyInjection;
using System;

namespace Otc.RequestTracking.AspNetCore
{
    public class RequestTrackerConfigurationLambda
    {
        private IServiceCollection services;

        public RequestTrackerConfigurationLambda(IServiceCollection services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public void Configure(RequestTrackerConfiguration requestTrackerConfiguration)
        {
            if (requestTrackerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(requestTrackerConfiguration));
            }

            services.AddSingleton(requestTrackerConfiguration);
        }
    }
}