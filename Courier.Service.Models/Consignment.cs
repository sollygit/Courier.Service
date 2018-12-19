namespace Courier.Service.Models
{
    public class Consignment
    {
        public string ConsignmentId { get; set; }
        public string ConsignmentURL { get; set; }
        public string Details { get; set; }

        public Consignment()
        {
            ConsignmentId = "NONE";
            ConsignmentURL = string.Empty;
            Details = string.Empty;
        }

        public Consignment(string consignmentId, string details = null)
        {
            ConsignmentId = consignmentId;
            ConsignmentURL = string.Empty;
            Details = details ?? string.Empty;
        }
    }
}
