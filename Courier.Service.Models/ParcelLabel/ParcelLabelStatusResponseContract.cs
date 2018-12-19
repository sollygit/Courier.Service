using System.Collections.Generic;

namespace Courier.Service.Models.ParcelLabel
{
    public class ParcelLabelStatusResponseContract
    {
        public bool Success { get; set; }
        public string Message_Id { get; set; }
        public string Consignment_Status { get; set; }
        public string Consignment_Url { get; set; }
        public string Expiry_Date_UTC { get; set; }
        public List<Error> Errors { get; set; }
    }
}
