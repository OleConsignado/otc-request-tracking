using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Otc.RequestTracking.AspNetCore
{
    public class RequestTracker
    {
        private readonly RequestTrackerConfiguration requestTrackerConfiguration;
        private readonly ILogger logger;

        public RequestTracker(ILoggerFactory loggerFactory, RequestTrackerConfiguration requestTrackerConfiguration)
        {
            this.requestTrackerConfiguration = requestTrackerConfiguration ?? throw new ArgumentNullException(nameof(requestTrackerConfiguration));
            logger = loggerFactory?.CreateLogger<RequestTracker>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private bool ShouldLogBody(HttpRequest request) => !string.IsNullOrEmpty(request.ContentType) &&
                Regex.IsMatch(request.ContentType, requestTrackerConfiguration.EnableBodyLoggingForContentType, RegexOptions.IgnoreCase) &&
                (string.IsNullOrEmpty(requestTrackerConfiguration.DisableBodyCapturingForUrl) ||
                !Regex.IsMatch(request.Path, requestTrackerConfiguration.DisableBodyCapturingForUrl, RegexOptions.IgnoreCase));


        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns>Returne parsed enconding or null if couldnt parse it</returns>
        private Encoding ParseContentEncoding(string contentType)
        {
            Encoding encoding = null;

            // try get the charset from:
            // application/json; charset=utf-8
            var tokens = contentType?.Split(';');

            if (tokens?.Length > 1)
            {
                var charsetToken = tokens[1]?.Split('=');

                if (charsetToken?[0].ToLower().Trim() == "charset")
                {
                    var charset = charsetToken?[1].ToUpper().Trim();

                    try
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException)
                    {
                        encoding = null;
                    }
                }
            }

            return encoding;
        }

        private bool IncludeExcludeTest(string excludePattern, string includePattern, string input)
        {
            var excludeUrlTest = !(excludePattern != null
                && Regex.IsMatch(input, excludePattern, RegexOptions.IgnoreCase));
            var includeUrlTest = includePattern != null
                && Regex.IsMatch(input, includePattern, RegexOptions.IgnoreCase);

            return excludeUrlTest || includeUrlTest;
        }

        /// <summary>
        /// Verify if the given <see cref="HttpRequest"/> should be logged in respect to <see cref="RequestTrackerConfiguration"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to log</param>
        /// <returns></returns>
        public bool ShouldLogRequest(HttpRequest request)
        {
            if (!requestTrackerConfiguration.RequestTrackerEnabled)
            {
                return false;
            }

            var urlTest = false;

            var httpMethodTest = IncludeExcludeTest(requestTrackerConfiguration.ExcludeHttpMethod, requestTrackerConfiguration.IncludeHttpMethod,
                request.Method);

            if(httpMethodTest)
            {
                urlTest = IncludeExcludeTest(requestTrackerConfiguration.ExcludeUrl, requestTrackerConfiguration.IncludeUrl,
                    request.Path + request.QueryString.ToUriComponent());
            }

            return httpMethodTest && urlTest;
        }

        public const int RequestLogEventId = 419012830;

        /// <summary>
        /// Log request if <see cref="ShouldLogRequest"/> returns true
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to log</param>
        public void LogRequestIfShouldLogIt(HttpRequest request)
        {
            if(ShouldLogRequest(request))
            {
                LogRequest(request);
            }
        }

        /// <summary>
        /// Perform log operation for the given <see cref="HttpRequest"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to log</param>
        public void LogRequest(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestLogModel = new LogModel()
            {
                Headers = ExtractHeaders(request),
                Method = StringUtil.TruncateIfLengthExceeds(request.Method, 10),
                Protocol = request.Protocol,
                Url = StringUtil.TruncateIfLengthExceeds(
                    $"{request.Scheme}://{request.Host.Value}{request.Path.Value}{request.QueryString.ToUriComponent()}",
                    requestTrackerConfiguration.UrlMaxLength),
                RemoteAddress = request.HttpContext.Connection?.RemoteIpAddress?.ToString()
            };

            if (ShouldLogBody(request))
            {
                Encoding encoding = ParseContentEncoding(request.ContentType) ?? Encoding.UTF8;
                requestLogModel.Body = ExtractBody(request, encoding);
            }

            logger.LogInformation(RequestLogEventId, "New request: {@Request}", requestLogModel);
        }

        private string ExtractBody(HttpRequest request, Encoding encoding)
        {
            request.EnableRewind();

            var buffer = new char[requestTrackerConfiguration.BodyMaxLength + 256];

            using (var reader = new StreamReader(
                   request.Body,
                   encoding: encoding,
                   detectEncodingFromByteOrderMarks: false,
                   bufferSize: buffer.Length,
                   leaveOpen: true))
            {
                int length = reader.ReadBlock(buffer, 0, buffer.Length);
                request.Body.Position = 0;
                var body = new string(buffer, 0, length);

                return StringUtil.TruncateIfLengthExceeds(body, requestTrackerConfiguration.BodyMaxLength);
            }
        }

        private IDictionary<string, string> ExtractHeaders(HttpRequest request)
        {
            var result = new Dictionary<string, string>();

            foreach (var header in request.Headers)
            {
                result.Add(
                    StringUtil.TruncateIfLengthExceeds(header.Key, requestTrackerConfiguration.HeaderNameMaxLength),
                    StringUtil.TruncateIfLengthExceeds(header.Value, requestTrackerConfiguration.HeaderValueMaxLength));
            }

            return result;
        }
    }
}
