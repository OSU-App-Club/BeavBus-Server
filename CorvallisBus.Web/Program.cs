using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CorvallisBus.Web
{
    /// <summary>
    /// Main Program
    /// </summary>
    public class Program
    {
        static async Task Main(string[] args)
        {
            await BuildWebHost(args).RunAsync();
        }

        /// <summary>
        /// Build Web Host
        /// </summary>
        /// <param name="args">CLI Arguments</param>
        /// <returns>IHost for Service</returns>
        public static IHost BuildWebHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
        }
    }
}
