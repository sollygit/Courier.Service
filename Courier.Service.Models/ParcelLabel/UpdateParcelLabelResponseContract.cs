namespace Courier.Service.Models.ParcelLabel
{
    public class UpdateParcelLabelResponseContract
    {
        public string TransactionId { get; set; }
        public string BranchID { get; set; }
        public string OrderNumber { get; set; }
        public ServiceResult ServiceResult { get; set; }

        public UpdateParcelLabelResponseContract()
        {
            TransactionId = string.Empty;
            BranchID = string.Empty;
            OrderNumber = string.Empty;
            ServiceResult = new ServiceResult { };
        }
    }
}
