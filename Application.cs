using System;

namespace WebServer
{
    public class HelloBinder : RequestBinder
    {
        public HelloBinder()
        {
            Get["/hello"] = (request, response) =>
            {
                response.StatusCode = 200;
                response.Body = "{\"success\":true, \"method\":\"GET\"}";
                response.Finish();
            };

            Post["/hello"] = (request, response) =>
            {
                response.StatusCode = 200;
                response.Body = "{\"success\":true, \"method\":\"POST\"}";
                response.Finish();
            };
        }
    }

    public class Application
    {
        static void Main()
        {
            new WebServer(
                new[] {"http://localhost:9999/"},
                new DefaultRequestHandler(new HelloBinder())).Run();

            Console.ReadKey();
        }
    }
}
