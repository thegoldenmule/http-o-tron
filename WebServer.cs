using System;
using System.Net;
using System.Text;
using System.Threading;

namespace WebServer
{
    /// <summary>
    /// Very basic web server.
    /// </summary>
    public class WebServer
    {
        /// <summary>
        /// HttpListener implementation.
        /// </summary>
        private readonly HttpListener _listener = new HttpListener();

        /// <summary>
        /// Handles requests.
        /// </summary>
        private readonly IRequestHandler _handler;

        /// <summary>
        /// Creates a new web server.
        /// </summary>
        /// <param name="prefixes">Prefixes this webserver binds to.</param>
        /// <param name="handler">The request handler to pass requests to.</param>
        public WebServer(
            string[] prefixes,
            IRequestHandler handler)
        {
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _handler = handler;
        }
        
        /// <summary>
        /// Starts the web server.
        /// </summary>
        public void Run()
        {
            _listener.Start();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Console.WriteLine("Webserver running...");
                
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(context =>
                    {
                        var ctx = (HttpListenerContext) context;

                        _handler.Process(
                            ctx.Request,
                            response =>
                            {
                                // apply response
                                ctx.Response.KeepAlive = false;
                                ctx.Response.StatusCode = response.StatusCode;
                                foreach (var header in response.Headers)
                                {
                                    ctx.Response.AddHeader(header.Key, header.Value);
                                }

                                if (!string.IsNullOrEmpty(response.Body))
                                {
                                    var bytes = Encoding.UTF8.GetBytes(response.Body);
                                    ctx.Response.OutputStream.Write(
                                        bytes,
                                        0,
                                        bytes.Length);
                                }

                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            },
                            exception =>
                            {
                                Console.WriteLine("Error!");
                            });

                    }, _listener.GetContext());
                }
            });
        }

        /// <summary>
        /// Stops the web server.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}