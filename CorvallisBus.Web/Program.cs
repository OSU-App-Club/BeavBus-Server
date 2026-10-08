using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CorvallisBus.Web
{
    /// <summary></summary>
    public class Program
    {
        static async Task Main(string[] args)
        {
            await BuildWebHost(args).RunAsync();
        }

        /// <summary>
        /// Build the ASP.NET Web Host using the Startup class
        /// </summary>
        public static IHost BuildWebHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
    }
}
