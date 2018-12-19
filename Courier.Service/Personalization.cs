using Courier.Service.Models.Notification;
using Newtonsoft.Json;

namespace Courier.Service
{
    public class Personalization : SendGrid.Helpers.Mail.Personalization
    {
        [JsonProperty("dynamic_template_data")]
        public NotificationRequest TemplateData { get; set; }
    }
}
