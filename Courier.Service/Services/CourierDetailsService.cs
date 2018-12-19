using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class CourierDetailsService : ICourierDetailsService
    {
        readonly ILogger<CourierDetailsService> logger;
        readonly CourierSettings courierSettings;

        public CourierDetailsService(ILogger<CourierDetailsService> logger, CourierSettings courierSettings)
        {
            this.logger = logger;
            this.courierSettings = courierSettings;
        }

        public async Task<CourierDetails> Get(int locationId, string deliveryType)
        {
            using (var client = new HttpClient())
            {
                var response = new HttpResponseMessage();
                var uriBuilder = new UriBuilder(string.Format(courierSettings.CourierDetailsUrl, locationId, deliveryType));

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                response = await client.GetAsync(uriBuilder.Uri);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Unsuccessful response in GetCourierDetails: {response.StatusCode}");
                    throw new ServiceException(response.StatusCode, $"CourierDetails with LocationId '{locationId}' and DeliveryType '{deliveryType}' could not be found");
                }

                var courierDetails = JsonConvert.DeserializeObject<CourierDetails>(response.Content.ReadAsStringAsync().Result);

                if (string.IsNullOrEmpty(courierDetails.Username) ||
                    string.IsNullOrEmpty(courierDetails.Recipient) ||
                    string.IsNullOrEmpty(courierDetails.SiteCode) || 
                    string.IsNullOrEmpty(courierDetails.ServiceCode))
                {
                    logger.LogError($"Unsuccessful response in GetCourierDetails: {response.StatusCode}");
                    throw new ServiceException($"CourierDetails are missing for LocationId '{locationId}' and DeliveryType '{deliveryType}'");
                }

                // Log Courier Details information
                logger.LogDebug($"Username:{courierDetails.Username}, ServiceCode:{courierDetails.ServiceCode}, SiteCode:{courierDetails.SiteCode }, LogoId:{courierDetails.LogoId}, Recipient:{courierDetails.Recipient},");

                return courierDetails;
            }
        }
    }
}
