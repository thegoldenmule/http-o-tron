using System;
using System.Net;

namespace WebServer
{
    public interface IRequestHandler
    {
        void Process(
            HttpListenerRequest request,
            Action<HttpResponse> success,
            Action<Exception> failure);
    }
}