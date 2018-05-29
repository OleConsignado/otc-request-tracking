using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text;

namespace Otc.RequestTracking.AspNetCore.Tests
{
    public class RequestTrackerTest
    {
        private TestServer CreateServer(LogEntryStore logEntryStore, RequestTrackerConfiguration requestTrackerConfiguration)
        {
            var webHostBuilder = new WebHostBuilder()
                            .ConfigureServices(services =>
                            {
                                services.AddTransient(c => logEntryStore);
                                services.AddScoped<ILoggerFactory, LoggerFactory>();
                                services.AddRequestTracking(requestTracking =>
                                {
                                    requestTracking.Configure(requestTrackerConfiguration);
                                });
                            })
                            .Configure(app =>
                            {
                                app.UseRequestTracking();

                                app.Run(async context =>
                                {
                                    context.Response.StatusCode = 200;
                                    var buffer = new byte[1024];
                                    int readBytes;
                                    var sb = new StringBuilder();                                  

                                    while ((readBytes = context.Request.Body.Read(buffer, 0, buffer.Length / 2)) > 0) 
                                    {
                                        bool decoded = false;
                                        int retries = 0;

                                        while (!decoded)
                                        {
                                            try
                                            {
                                                if(retries > 5)
                                                {
                                                    throw new InvalidOperationException("Trying to decode as UTF8 a non UTF8 content.");
                                                }

                                                var decodedString = Encoding.UTF8.GetString(buffer, 0, readBytes);
                                                decoded = true;
                                                sb.Append(decodedString);
                                            }
                                            catch (ArgumentException)
                                            {
                                                readBytes += context.Request.Body.Read(buffer, readBytes, 1); // try to read one more byte
                                                retries++;
                                            }
                                        }

                                    }

                                    await context.Response.WriteAsync(sb.ToString());
                                });
                            });

            return new TestServer(webHostBuilder);
        }

        [Fact]
        public async Task Test_IncludeExcludeHttpMethod()
        {
            var logEntryStore = new LogEntryStore();

            var requestTrackerConfiguration = new RequestTrackerConfiguration()
            {
                RequestTrackerEnabled = true,
                ExcludeHttpMethod = ".*",
                IncludeHttpMethod = "^post"
            };

            using (var server = CreateServer(logEntryStore, requestTrackerConfiguration))
            {
                logEntryStore.Clear();
                await server.CreateRequest("/").GetAsync();
                Assert.Null(logEntryStore.LogEntry);

                logEntryStore.Clear();
                await server.CreateRequest("/my").PostAsync();
                Assert.Contains("/my", logEntryStore.LogEntry.Url);
            }
        }

        [Fact]
        public async Task Test_IncludeExcludeUrl()
        {
            var logEntryStore = new LogEntryStore();

            var requestTrackerConfiguration = new RequestTrackerConfiguration()
            {
                RequestTrackerEnabled = true,
                ExcludeUrl = "^/api|^/favicon",
                IncludeUrl = "^/api/log01|^/api/log02"
            };

            using (var server = CreateServer(logEntryStore, requestTrackerConfiguration))
            {
                logEntryStore.Clear();
                await server.CreateRequest("/api").GetAsync();
                Assert.Null(logEntryStore.LogEntry);

                logEntryStore.Clear();
                await server.CreateRequest("/favicon").GetAsync();
                Assert.Null(logEntryStore.LogEntry);

                logEntryStore.Clear();
                await server.CreateRequest("/xyz/favicon").GetAsync();
                Assert.Contains("/xyz/favicon", logEntryStore.LogEntry.Url);

                logEntryStore.Clear();
                await server.CreateRequest("/api/log01").GetAsync();
                Assert.Contains("/log01", logEntryStore.LogEntry.Url);

                logEntryStore.Clear();
                await server.CreateRequest("/api/log01/ABC").GetAsync();
                Assert.Contains("/api/log01/ABC", logEntryStore.LogEntry.Url);

                logEntryStore.Clear();
                await server.CreateRequest("/api/log02/XYZ").GetAsync();
                Assert.Contains("/api/log02/XYZ", logEntryStore.LogEntry.Url);
            }
        }

        [Fact]
        public async Task Test_LogPayLoad()
        {
            var logEntryStore = new LogEntryStore();
            var bodyMaxLength = 256;

            var requestTrackerConfiguration = new RequestTrackerConfiguration()
            {
                RequestTrackerEnabled = true,
                BodyMaxLength = bodyMaxLength
            };

            using (var server = CreateServer(logEntryStore, requestTrackerConfiguration))
            {
                var sb = new StringBuilder();
                
                var rand = new Random();
                sb.Append("{");

                for (int i = 0; i < 1024; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append($"{{ \"{rand.Next(0xffff, int.MaxValue):X}\": \"{rand.Next(0xffff, int.MaxValue):X}\" }}");
                }

                sb.Append("}");

                var body = sb.ToString();

                var response = await server.CreateRequest("/api/testpost")
                    .And(config =>
                    {
                        config.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    })
                    .PostAsync();

                Assert.Equal(body, await response.Content.ReadAsStringAsync());
                Assert.NotEqual(body, logEntryStore.LogEntry.Body); // diff because logEntryStore.LogEntry.Body is trucated
                Assert.StartsWith(body.Substring(0, bodyMaxLength - 50) // (-50) => StringUtil.TruncateIfLengthExceeds discount suffixIfTruncated length
                    , logEntryStore.LogEntry.Body);
            }
        }
    }
}
