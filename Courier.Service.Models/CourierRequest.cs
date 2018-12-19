using Courier.Service.Models.ParcelLabel;
using Courier.Service.Models.ParcelPickup;
using System;

namespace Courier.Service.Models
{
    public class CourierRequest
    {
        public int BranchId { get; set; }
        public string DeliveryType { get; set; }
        public string FullOrderNumber { get; set; }
        public string Carrier { get; set; }
        public string Caller { get; set; }
        public DateTime Parcel_Pickup_Date_Time { get; set; }
        public int Parcel_Quantity { get; set; }
        public PickupAddress Parcel_Pickup_Address { get; set; }
        public DeliveryAddress Parcel_Delivery_Address { get; set; }
        public SenderDetails Label_Sender_Details { get; set; }
        public ReceiverDetails Label_Receiver_Details { get; set; }
        public Address Label_Delivery_Address { get; set; }
    }
}
