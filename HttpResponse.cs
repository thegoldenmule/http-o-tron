using System;
using System.Collections.Generic;

namespace WebServer
{
    /// <summary>
    /// Response object.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Default status code.
        /// </summary>
        public int StatusCode = 404;

        /// <summary>
        /// The response body.
        /// </summary>
        public string Body;

        /// <summary>
        /// Header information.
        /// </summary>
        public readonly Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// Call when finished processing.
        /// </summary>
        public readonly Action Finish;
        
        /// <summary>
        /// Creates a new Response object.
        /// </summary>
        public HttpResponse(Action<HttpResponse> succeed)
        {
            Finish = () => succeed(this);
        }
    }
}