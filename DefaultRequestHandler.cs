using System;
using System.Net;

namespace WebServer
{
    /// <summary>
    /// Default implementation of IRequestHandler.
    /// </summary>
    public class DefaultRequestHandler : IRequestHandler
    {
        /// <summary>
        /// Collection of binders to search through.
        /// </summary>
        private readonly RequestBinder[] _binders;

        /// <summary>
        /// Creates a new request handler.
        /// </summary>
        /// <param name="binders">Binders.</param>
        public DefaultRequestHandler(params RequestBinder[] binders)
        {
            _binders = binders;
        }

        /// <inheritdoc cref="IRequestHandler"/>
        public void Process(
            HttpListenerRequest request,
            Action<HttpResponse> success,
            Action<Exception> failure)
        {
            // automatically handle preflight CORS requests
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS
            if (request.HttpMethod == "OPTIONS")
            {
                ProcessPreflight(
                    request,
                    success,
                    failure);
                return;
            }

            var endpoint = request.Url.LocalPath;
            Console.WriteLine("Received request : {0}.", endpoint);
            
            // prep response
            var response = new HttpResponse(success);
            AddHeaders(response);

            // get the appropriate responder
            GetResponder(request.HttpMethod, endpoint)
                .Invoke(
                    new HttpRequest(request),
                    response);
        }

        /// <summary>
        /// Retrieves the responder for a method and endpoint.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        protected RequestMethodBinder.Responder GetResponder(
            string method,
            string endpoint)
        {
            switch (method)
            {
                case "POST":
                {
                    foreach (var binder in _binders)
                    {
                        var responder = binder.Post.Match(endpoint);
                        if (null != responder)
                        {
                            return responder;
                        }
                    }

                    break;
                }

                case "GET":
                {
                    foreach (var binder in _binders)
                    {
                        var responder = binder.Get.Match(endpoint);
                        if (null != responder)
                        {
                            return responder;
                        }
                    }

                        break;
                }
            }

            return (request, response) => response.Finish();
        }

        /// <summary>
        /// Processes preflight CORS requests.
        /// </summary>
        protected void ProcessPreflight(
            HttpListenerRequest request,
            Action<HttpResponse> success,
            Action<Exception> failure)
        {
            var response = new HttpResponse(success)
            {
                StatusCode = 200
            };

            AddHeaders(response);

            response.Finish();
        }

        /// <summary>
        /// Adds default headers.
        /// </summary>
        /// <param name="response">Response to add headers to.</param>
        protected void AddHeaders(HttpResponse response)
        {
            response.Headers.Add(
                "Access-Control-Allow-Methods",
                "POST, GET, OPTIONS");
            response.Headers.Add(
                "Access-Control-Allow-Origin",
                "*");
            response.Headers.Add(
                "Access-Control-Allow-Headers",
                "origin, content-type, cache-control, x-requested-with");
        }
    }
}