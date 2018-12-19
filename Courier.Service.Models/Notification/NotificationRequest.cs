namespace Courier.Service.Models.Notification
{
    public class NotificationRequest
    {
        public string TransactionId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string OrderNo { get; set; }
        public string CustomerId { get; set; }
        public string CustomerEmail { get; set; }
        public string NotificationType { get; set; }
        public string NotificationMethod { get; set; }
        public string OrderLocation { get; set; }
    }
}
