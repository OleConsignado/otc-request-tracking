using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Otc.RequestTracking.AspNetCore;
using Serilog;
using Serilog.Formatting.Json;

namespace RequestTrackingSampleApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.Console(new JsonFormatter()))
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(Configuration)
                //.MinimumLevel.Override("Otc.RequestTracker.AspNetCore", LogEventLevel.Information)
                .CreateLogger();

            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddSerilog();
            });

            services.AddRequestTracking(requestTracker =>
            {
                requestTracker.Configure(new RequestTrackerConfiguration()
                {
                    RequestTrackerEnabled = true
                });
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRequestTracking();
            app.UseMvc();
        }
    }
}
