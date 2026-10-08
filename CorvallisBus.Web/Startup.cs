using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CorvallisBus.Core;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CorvallisBus.Web
{
    internal class Startup
    {
        private const string AppDescription = @"
The REST API that powers the BeavBus Corvallis Transit System data.

Check it out on GitHub: [github.com/OSU-App-Club/BeavBus-Server](https://github.com/OSU-App-Club/BeavBus-Server)

### Summary

The Corvallis Bus REST API provides a convenient way to get real-time information about the free buses in Corvallis.
Data from CTS is merged with data from Google Transit, with some convenient projections applied, and mapped into some easily-digestable JSON for different use cases.

See the official BeavBus Client: [github.com/OSU-App-Club/beavbus](https://github.com/OSU-App-Club/beavbus)

### Disclaimer

We assume no liability for any missed buses.
Buses may be erratic in their arrival behavior, and we cannot control that.";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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
            services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(
                    new JsonStringEnumConverter<TransitSystem>());
                options.SerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;

            });
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter<TransitSystem>());
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase;
                });
            services.AddMvc(option => option.EnableEndpointRouting = false);

            services.AddSingleton<ITransitRepository, MemoryTransitRepository>();
            services.AddHttpClient<ITransitClient, TransitClient>();
            services.AddHostedService<Worker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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