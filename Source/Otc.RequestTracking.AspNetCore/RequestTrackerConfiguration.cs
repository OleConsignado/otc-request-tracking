namespace Otc.RequestTracking.AspNetCore
{
    public class RequestTrackerConfiguration
    {
        /// <summary>
        /// Enable log request details
        /// </summary>
        public bool RequestTrackerEnabled = true;

        /// <summary>
        /// Max length for header key in logs (exceeds will be truncated)
        /// </summary>
        public int HeaderNameMaxLength { get; set; } = 256;

        /// <summary>
        /// Max length for header value in logs (exceeds will be truncated)
        /// </summary>
        public int HeaderValueMaxLength  { get; set; }  = 4096;

        /// <summary>
        /// Max length for request body in logs (exceeds will be truncated)
        /// </summary>
        public int BodyMaxLength { get; set; } = 8192;

        /// <summary>
        /// Max length for Url (including schema, host, path and querystring) in logs (exceeds will be truncated)
        /// </summary>
        public int UrlMaxLength { get; set; } = 2083;

        /// <summary>
        /// Url to not log (case insensitive regex pattern). 
        /// <para>
        /// Only the portion after host/port, including querystring will be analyzed, in other words, the path + querystring.
        /// </para>
        /// <para>
        /// The exclude url role will be applied before to <see cref="IncludeUrl"/>.
        /// </para>
        /// </summary>
        public string ExcludeUrl { get; set; }

        /// <summary>
        /// Url to log (case insensitive regex pattern).
        /// <para>
        /// Only the portion after host/port, including querystring will be analyzed, in other words, the path + querystring.
        /// </para>
        /// <para>
        /// By default, all url will be included in logs. If you want to enable only a specific url (ex: /PathToLog), you must set <see cref="ExcludeUrl"/> as '^/' and <see cref="IncludeUrl"/> as '^/PathToLog'
        /// </para>
        /// </summary>
        public string IncludeUrl { get; set; }

        /// <summary>
        /// Enable body (request payload) logging when content-type string matches pattern (case insensitive regex); default: ".*?/(json|x-www-form-urlencoded);"
        /// </summary>
        public string EnableBodyLoggingForContentType { get; set; } = ".*?/(json|x-www-form-urlencoded)";

        /// <summary>
        /// Http method to not log (case insensitive regex).
        /// <para>
        /// By default (ExcludeHttpMethod == null) all http methods will be logged. This configuration, together <see cref="IncludeHttpMethod"/>, should be used 
        /// like <see cref="IncludeUrl"/> and <see cref="ExcludeUrl"/> but for http method (verb).
        /// </para>
        /// </summary>
        public string ExcludeHttpMethod { get; set; }

        /// <summary>
        /// Http method to log (case insensitive regex).
        /// <para>
        /// This configuration, together <see cref="ExcludeHttpMethod"/>, should be used like <see cref="IncludeUrl"/> and <see cref="ExcludeUrl"/> but for http method (verb).
        /// </para>
        /// </summary>
        public string IncludeHttpMethod { get; set; }
    }
}
