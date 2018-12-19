using Courier.Service.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Courier.Service.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly JsonSerializerSettings serializerSettings;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
            serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await FormatException(context.Response, ex);
            }
        }

        private Task FormatException(HttpResponse response, Exception ex)
        {
            if (ex is NotSupportedException || ex is NotImplementedException)
            {
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
            }
            else if (ex is ServiceException)
            {
                response.StatusCode = (int)(ex as ServiceException).StatusCode;
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(new
            {
                ex.Message
            }, serializerSettings);
            return response.WriteAsync(result);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
