using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.ParcelLabel;
using Courier.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class ACEService : IACEService
    {
        readonly ILogger<ACEService> logger;
        readonly CourierSettings courierSettings;

        public ACEService(ILogger<ACEService> logger, CourierSettings courierSettings, AuthZeroSettings auth0Settings)
        {
            this.logger = logger;
            this.courierSettings = courierSettings;
        }

        public async Task<string> UpdateParcelLabel(string locationId, string orderNumber, Consignment consignment, string username)
        {
            var isValidConsignment = !string.IsNullOrEmpty(consignment.ConsignmentURL);

            var request = new UpdateParcelLabelRequest {
                TransactionId = DateTime.Now.Ticks.ToString(),
                BranchId = locationId,
                OrderNumber = orderNumber,
                ConsignmentUrl = isValidConsignment ? string.Format(courierSettings.EmailLabelDownloadUrl, consignment.ConsignmentId, username) : string.Empty,
                Message = consignment.Details ?? string.Empty
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", courierSettings.SubscriptionKey);

                var uriBuilder = new UriBuilder(courierSettings.UpdateParcelLabelUrl);
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None);
                var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uriBuilder.Uri, jsonContent);
                var result = response.Content.ReadAsStringAsync().Result;
                var contract = JsonConvert.DeserializeObject<UpdateParcelLabelResponseContract>(result);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"ACE Parcel Label Update StatusCode: {response.StatusCode}");
                }

                if (contract.ServiceResult.Code != 0)
                {
                    logger.LogError($"ACE Parcel Label Update Error Message: {contract.ServiceResult.Message}");
                }

                if (isValidConsignment)
                {
                    logger.LogDebug($"OrderNumber:{request.OrderNumber} ConsignmentUrl:{request.ConsignmentUrl}");
                }

                else
                {
                    logger.LogDebug($"OrderNumber:{request.OrderNumber} Message:{request.Message}");
                }

                return contract.OrderNumber;
            }
        }
    }
}
