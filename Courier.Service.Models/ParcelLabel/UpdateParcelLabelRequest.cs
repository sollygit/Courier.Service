using Newtonsoft.Json;

namespace Courier.Service.Models.ParcelLabel
{
    public class UpdateParcelLabelRequest
    {
        public string TransactionId { get; set; }
        public string BranchId { get; set; }
        public string OrderNumber { get; set; }
        public string ConsignmentUrl { get; set; }
        public string Message { get; set; }
    }
}
