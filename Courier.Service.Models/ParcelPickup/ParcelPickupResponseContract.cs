using System.Collections.Generic;

namespace Courier.Service.Models.ParcelPickup
{
    public class ParcelPickupResponseContract
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public Results Results { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class Results
    {
        public string ResponseType { get; set; }
        public string JobId { get; set; }
        public string JobNumber { get; set; }
        public string CustomerReference { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
