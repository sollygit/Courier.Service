using Newtonsoft.Json;
using System.Collections.Generic;

namespace Courier.Service.Models.ParcelLabel
{
    public class ParcelLabelRequest
    {
        public string Carrier { get; set; }
        public string Orientation { get; set; } = "LANDSCAPE";
        public string Format { get; set; } = "PDF";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Logo_Id { get; set; }

        public int Job_Number { get; set; }
        public SenderDetails Sender_Details { get; set; }
        public PickAddress Pickup_Address { get; set; }
        public ReceiverDetails Receiver_Details { get; set; }
        public Address Delivery_Address { get; set; }
        public List<ParcelDetail> Parcel_Details { get; set; }
    }
}
