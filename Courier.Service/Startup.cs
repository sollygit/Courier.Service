using Courier.Service.Api;
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
            services.AddMemoryCache();

            services.AddSingleton(provider => Configuration.GetSection("Auth0").Get<AuthZeroSettings>());
            services.AddSingleton(provider => Configuration.GetSection("CourierSettings").Get<CourierSettings>());
            services.AddSingleton(provider => Configuration.GetSection("SendGridSettings").Get<SendGridSettings>());
            services.AddTransient<IAuthService, AuthZeroService>();
            
            // Register Hosted Services
            services.AddSingleton<IHostedService, RequestHandlerService>();
            services.AddSingleton<ICourierService<CourierRequest>, CourierService>();
            services.AddTransient<ICourierDetailsService, CourierDetailsService>();
            services.AddTransient<IParcelPickupService, ParcelPickupService>();
            services.AddTransient<IParcelLabelService, ParcelLabelService>();
            services.AddTransient<IACEService, ACEService>();
            services.AddTransient<INotificationService, NotificationService>();

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
