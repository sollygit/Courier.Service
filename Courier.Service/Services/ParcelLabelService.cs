using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.ParcelLabel;
using Courier.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class ParcelLabelService : IParcelLabelService
    {
        private readonly ILogger<ParcelLabelService> logger;
        private readonly IAuthService authService;
        private readonly CourierSettings courierSettings;
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings serializerSettings;

        public ParcelLabelService(
            ILogger<ParcelLabelService> logger, 
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

        public async Task<Consignment> Create(ParcelLabelRequest request, CourierDetails courierDetails)
        {
            var token = await authService.GetToken();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            httpClient.DefaultRequestHeaders.Add("user_name", courierDetails.Username);

            request.Pickup_Address.Country_Code = courierSettings.CountryCode;
            request.Delivery_Address.Country_Code = courierSettings.CountryCode;
            request.Parcel_Details = new List<ParcelDetail> { new ParcelDetail(courierDetails.ServiceCode) };

            var uriBuilder = new UriBuilder(courierSettings.ParcelLabelUrl);
            var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, serializerSettings);
            var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(uriBuilder.Uri, jsonContent);
            var result = response.Content.ReadAsStringAsync().Result;
            var contract = JsonConvert.DeserializeObject<ParcelLabelResponseContract>(result, serializerSettings);

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(contract.Consignment_Id))
            {
                logger.LogError($"Unsuccessful Parcel Label Create: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Parcel Label Create Error - {contract.Errors[0].Details}");
            }

            logger.LogDebug($"Parcel Label Create Job_Number:{request.Job_Number} Consignment_Id:{contract.Consignment_Id}");

            // Include any errors/wanrnings related to parcel label create
            if (contract.Errors != null && contract.Errors.Count != 0)
            {
                logger.LogWarning($"Consignment_Id:{contract.Consignment_Id} Details:{contract.Errors[0].Details}");
                return new Consignment(contract.Consignment_Id, contract.Errors[0].Details);
            }

            return new Consignment(contract.Consignment_Id);
        }

        public async Task<string> GetStatus(string consignmentId, CourierDetails courierDetails)
        {
            var count = 1;
            var contract = await GetStatusResponse(consignmentId, courierDetails);

            // Keep polling for the consignment status
            while (string.IsNullOrEmpty(contract.Consignment_Url) && count++ <= courierSettings.MaxAttempt)
            {
                contract = await GetStatusResponse(consignmentId, courierDetails);
            }

            if (string.IsNullOrEmpty(contract.Consignment_Url))
            {
                throw new ServiceException($"Parcel Label Status {contract.Consignment_Status} for ConsignmentId {consignmentId} after {count-1} attempts.");
            }

            return contract.Consignment_Url;
        }

        public async Task<byte[]> Download(string consignmentId, string username)
        {
            var token = await authService.GetToken();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{courierSettings.ParcelLabelUrl}/{consignmentId}");

            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("user_name", username);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Label Download Error: ConsignmentId {consignmentId}: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Label Download Error: ConsignmentId {consignmentId}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<ParcelLabelStatusResponseContract> GetStatusResponse(string consignmentId, CourierDetails courierDetails)
        {
            // Wait a few seconds to ensure process of label
            await Task.Delay(courierSettings.MilliSecondsDelay);

            var token = await authService.GetToken();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            httpClient.DefaultRequestHeaders.Add("user_name", courierDetails.Username);

            var uriBuilder = new UriBuilder($"{courierSettings.ParcelLabelUrl}/{consignmentId}/status");
            var response = await httpClient.GetAsync(uriBuilder.Uri);
            var result = response.Content.ReadAsStringAsync().Result;
            var contract = JsonConvert.DeserializeObject<ParcelLabelStatusResponseContract>(result, serializerSettings);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Unsuccessful Parcel Label Status with ConsignmentId {consignmentId}: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Parcel Label Status Error for ConsignmentId {consignmentId}: {contract.Errors[0].Details}");
            }

            logger.LogDebug($"ConsignmentId:{consignmentId} Status {contract.Consignment_Status}. {contract.Consignment_Url}");

            return contract;
        }
    }
}
