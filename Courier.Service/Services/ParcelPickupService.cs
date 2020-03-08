using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.ParcelPickup;
using Courier.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class ParcelPickupService : IParcelPickupService
    {
        private readonly ILogger<ParcelPickupService> logger;
        private readonly IAuthService authService;
        private readonly CourierSettings courierSettings;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings serializerSettings;

        public ParcelPickupService(ILogger<ParcelPickupService> logger,
            IAuthService authService, 
            CourierSettings courierSettings,
            HttpClient httpClient)
        {
            this.logger = logger;
            this.authService = authService;
            this.courierSettings = courierSettings;
            this.httpClient = httpClient;

            serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
            };
        }

        public async Task<string> ParcelPickup(ParcelPickupRequest request, CourierDetails courierDetails)
        {
            var token = await authService.GetToken();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            httpClient.DefaultRequestHeaders.Add("user_name", courierDetails.Username);

            var uriBuilder = new UriBuilder(courierSettings.ParcelPickupUrl);
            var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, serializerSettings);
            var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(uriBuilder.Uri, jsonContent);
            var result = response.Content.ReadAsStringAsync().Result;
            var contract = JsonConvert.DeserializeObject<ParcelPickupResponseContract>(result, serializerSettings);

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(contract.Results.JobNumber))
            {
                logger.LogError($"Unsuccessful Parcel Pickup response: {response.StatusCode}");

                if (contract.Errors != null && contract.Errors.Count != 0)
                {
                    throw new ServiceException(response.StatusCode, $"Parcel Pickup Error - {contract.Errors[0].Details}");
                }
                else
                {
                    throw new ServiceException(response.StatusCode, $"Parcel Pickup Status Code: {response.StatusCode}");
                }
            }

            logger.LogDebug($"Parcel Pickup Job_Number:{contract.Results.JobNumber}");

            return contract.Results.JobNumber;
        }
    }
}
