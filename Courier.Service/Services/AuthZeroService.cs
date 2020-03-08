using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class AuthZeroService : IAuthService
    {
        private readonly ILogger<AuthZeroService> logger;
        private readonly IMemoryCache cache;
        private readonly AuthZeroSettings settings;
        private readonly JsonSerializerSettings serializerSettings;
        private readonly HttpClient httpClient;

        public AuthZeroService(
            AuthZeroSettings settings, 
            ILogger<AuthZeroService> logger, 
            IMemoryCache cache,
            HttpClient httpClient)
        {
            this.settings = settings;
            this.logger = logger;
            this.cache = cache;
            this.httpClient = httpClient;
            serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public Task<string> GetToken()
        {
            return cache.GetOrCreateAsync("auth0_token", async entry =>
            {
                var token = await GenerateToken();
                entry.SlidingExpiration = TimeSpan.FromSeconds(token.ExpiresIn);
                return token.AccessToken;
            });
        }

        private async Task<TokenResponse> GenerateToken()
        {
            var uriBuilder = new UriBuilder(settings.TokenEndPoint)
            {
                Query = $"grant_type={settings.GrantType}&client_id={settings.ClientId}&client_secret={settings.ClientSecret}"
            };

            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(uriBuilder.Uri, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("NZPOST Token oauth2 returned error code {0} when trying to retrieve the token. Additional details: {1}", response.StatusCode, errorContent);
                throw new ServiceException(HttpStatusCode.InternalServerError, "Could not retrieve token from NZPOST, please check logs to see further details");
            }
            return JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync(), serializerSettings);
        }
    }
}
