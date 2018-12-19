namespace Courier.Service.Models.Notification
{
    public class NotificationResponseContract
    {
        public string TransactionId { get; set; }
        public string OrderNo { get; set; }
        public ServiceResult ServiceResult { get; set; }

        public NotificationResponseContract()
        {
            TransactionId = string.Empty;
            OrderNo = string.Empty;
            ServiceResult = new ServiceResult { };
        }
    }
}
