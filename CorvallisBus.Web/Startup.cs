using CorvallisBus.Core;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.WebClients;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CorvallisBus.Web
{
    /// <summary>
    /// Startup Class for Corvallis Bus Server
    /// </summary>
    public class Startup(IConfiguration configuration)
    {
        /// <summary>
        /// Swagger/OpenAPI Description. Shown in the API documentation
        /// </summary>
        public const string AppDescription = @"
The REST API that powers the BeavBus Corvallis Transit System data.

Check it out on GitHub: [https://github.com/OSU-App-Club/BeavBus-Server](https://github.com/OSU-App-Club/BeavBus-Server)

### Summary

The Corvallis Bus REST API provides a convenient way to get real-time information about the free buses in Corvallis.
Data from CTS is merged with data from Google Transit, with some convenient projections applied, and mapped into some easily-digestable JSON for different use cases.

See the official BeavBus Client: [https://github.com/OSU-App-Club/beavbus](https://github.com/OSU-App-Club/beavbus)

### Disclaimer

We assume no liability for any missed buses.
Buses may be erratic in their arrival behavior, and we cannot control that.";

        /// <summary>
        /// Configuration Provider for App
        /// </summary>
        public IConfiguration Configuration { get; } = configuration;

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Service Collection for DI</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "BeavBusTransitClient";

                    document.Info.Description = AppDescription;

                    document.Info.License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Identifier = "MIT",
                    };

                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "OSU App Club",
                        Email = "appdevelopment.clubs@oregonstate.edu"
                    };
                    return Task.CompletedTask;
                });
            });
            services.AddMvc(option => option.EnableEndpointRouting = false);

            // FIXME: This should not require GetService like this
            services.AddSingleton<ITransitRepository>(provider => {
                IWebHostEnvironment? env = provider.GetService<IWebHostEnvironment>();
                string Path = env != null ? env.WebRootPath : "";
                return new MemoryTransitRepository(Path);
            });
            services.AddSingleton<ITransitClient, TransitClient>();

            services.AddHostedService<Worker>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder</param>
        /// <param name="env">Application Environment Variables</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapOpenApi("/openapi/{documentName}.yaml"));
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.yaml", "v1"));

            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseDefaultFiles();
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    ServeUnknownFileTypes = true
                });
            app.UseMvc();
        }
    }
}
