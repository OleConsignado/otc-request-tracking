using System.Collections.Generic;


namespace Otc.RequestTracking.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    public class LogModel
    {
        public string Method { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string Protocol { get; set; }
        public string Url { get;set; }       
        public string RemoteAddress { get; set; }
    }
}
