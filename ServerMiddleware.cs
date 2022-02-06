using Microsoft.Extensions.Caching.Distributed;

namespace IPSuiteCalculator
{
    public class ServerMiddleware
    {
        private RequestDelegate next;
        public ServerMiddleware(RequestDelegate nextDelgate)
        {
            next = nextDelgate;
        }
        public async Task Invoke(HttpContext context, IConfiguration config, IDistributedCache cache)
        {
            Thread t = new Thread(delegate ()
            {
                Server server = new Server(config["IPAddress"], config["Port"], cache);
            });
            t.Start();

            Console.WriteLine("Server Started...!");

            await next(context);
        }

        
    }
}