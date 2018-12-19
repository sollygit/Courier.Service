namespace Courier.Service.Settings
{
    public class CourierSettings
    {
        public string SubscriptionKey { get; set; }
        public int MilliSecondsDelay { get; set; }
        public int MaxAttempt { get; set; }
        public string CountryCode { get; set; }
        public string CourierDetailsUrl { get; set; }
        public string ParcelPickupUrl { get; set; }
        public string ParcelLabelUrl { get; set; }
        public string UpdateParcelLabelUrl { get; set; }
        public string NotificationUrl { get; set; }
        public string EmailLabelDownloadUrl { get; set; }
    }
}
