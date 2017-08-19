using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OidCredentials
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            // https://joonasw.net/view/aspnet-core-2-configuration-changes

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
