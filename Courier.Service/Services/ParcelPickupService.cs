using Courier.Service.Api;
using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.ParcelPickup;
using Courier.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class ParcelPickupService : IParcelPickupService
    {
        readonly ILogger<ParcelPickupService> logger;
        readonly IAuthService authService;
        readonly CourierSettings courierSettings;
        readonly AuthZeroSettings authZeroSettings;
        readonly JsonSerializerSettings serializerSettings;

        public ParcelPickupService(ILogger<ParcelPickupService> logger, IAuthService authService, CourierSettings courierSettings, AuthZeroSettings authZeroSettings)
        {
            this.logger = logger;
            this.authService = authService;
            this.courierSettings = courierSettings;
            this.authZeroSettings = authZeroSettings;

            serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
            };
        }

        public async Task<string> ParcelPickup(ParcelPickupRequest request, CourierDetails courierDetails)
        {
            using (var client = new HttpClient())
            {
                var token = await authService.GetToken();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("client_id", authZeroSettings.ClientId);
                client.DefaultRequestHeaders.Add("user_name", courierDetails.Username);

                var uriBuilder = new UriBuilder(courierSettings.ParcelPickupUrl);
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, serializerSettings);
                var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uriBuilder.Uri, jsonContent);
                var result = response.Content.ReadAsStringAsync().Result;
                var contract = JsonConvert.DeserializeObject<ParcelPickupResponseContract>(result, serializerSettings);

                if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(contract.Results.JobNumber))
                {
                    logger.LogError($"Unsuccessful Parcel Pickup response: {response.StatusCode}");
                    throw new ServiceException(response.StatusCode, $"Parcel Pickup Error - {contract.Errors[0].Details}");
                }

                logger.LogDebug($"Parcel Pickup Job_Number:{contract.Results.JobNumber}");

                return contract.Results.JobNumber;
            }
        }
    }
}
