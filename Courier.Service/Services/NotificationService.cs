using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.Notification;
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
    public class NotificationService : INotificationService
    {
        readonly ILogger<NotificationService> logger;
        readonly SendGridSettings sendGridSettings;
        readonly CourierSettings courierSettings;

        public NotificationService(ILogger<NotificationService> logger, SendGridSettings sendGridSettings, CourierSettings courierSettings)
        {
            this.logger = logger;
            this.sendGridSettings = sendGridSettings;
            this.courierSettings = courierSettings;
        }

        public async Task<string> Send(NotificationRequest request, string username, string consignmentId)
        {
            request.NotificationType = sendGridSettings.NotificationType;
            request.NotificationMethod = sendGridSettings.NotificationMethod;
            request.OrderLocation = string.Format(courierSettings.EmailLabelDownloadUrl, consignmentId, username);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", courierSettings.SubscriptionKey);

                var uriBuilder = new UriBuilder(string.Format(courierSettings.NotificationUrl, request.BranchId));
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None);
                var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uriBuilder.Uri, jsonContent);
                var result = response.Content.ReadAsStringAsync().Result;
                var contract = JsonConvert.DeserializeObject<NotificationResponseContract>(result);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Send Notification StatusCode: {response.StatusCode}");
                    throw new ServiceException(response.StatusCode, "Send Notification Error");
                }

                if (contract.ServiceResult.Code != 0)
                {
                    logger.LogError($"Send Notification: {contract.ServiceResult.Message}");
                    throw new ServiceException($"Send Notification: {contract.ServiceResult.Message}");
                }

                logger.LogDebug($"Label Notification for OrderNo {contract.OrderNo} was sent to {request.CustomerEmail}");
                logger.LogDebug($"Label Download URL:{request.OrderLocation}");

                return contract.OrderNo;
            }           
        }
    }
}
