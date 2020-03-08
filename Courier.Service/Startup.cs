using Courier.Service.Interfaces;
using Courier.Service.Middleware;
using Courier.Service.Models;
using Courier.Service.Services;
using Courier.Service.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.Net.Http.Headers;

namespace Courier.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var authSettings = Configuration.GetSection("Auth0").Get<AuthZeroSettings>();
            var courierSettings = Configuration.GetSection("CourierSettings").Get<CourierSettings>();
            var sendGridSettings = Configuration.GetSection("SendGridSettings").Get<SendGridSettings>();

            // Register Hosted Services
            services.AddMemoryCache();
            services.AddSingleton(provider => authSettings);
            services.AddSingleton(provider => courierSettings);
            services.AddSingleton(provider => sendGridSettings);
            services.AddSingleton<IHostedService, RequestHandlerService>();
            services.AddSingleton<ICourierService<CourierRequest>, CourierService>();

            services.AddHttpClient<ICourierDetailsService, CourierDetailsService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });
            services.AddHttpClient<IParcelPickupService, ParcelPickupService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("client_id", authSettings.ClientId);
            });
            services.AddHttpClient<IParcelLabelService, ParcelLabelService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("client_id", authSettings.ClientId);
            });
            services.AddHttpClient<IACEService, ACEService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", courierSettings.SubscriptionKey);
            });
            services.AddHttpClient<INotificationService, NotificationService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", courierSettings.SubscriptionKey);
            });
            services.AddHttpClient<IAuthService, AuthZeroService>(client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddJsonFormatters(settings =>
                {
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    settings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
                    settings.Converters.Add(new StringEnumConverter());
                });
                
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Courier Service API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, Microsoft.Extensions.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseJsonExceptionHandler();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Courier Service API");
            });

            app.UseMvc();
        }
    }
}
