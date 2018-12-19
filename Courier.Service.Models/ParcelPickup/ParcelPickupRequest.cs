using System;

namespace Courier.Service.Models.ParcelPickup
{
    public class ParcelPickupRequest
    {
        public string Carrier { get; set; }
        public string Caller { get; set; }
        public string Service_Code { get; set; }
        public DateTime Pickup_Date_Time { get; set; }
        public int Parcel_Quantity { get; set; }
        public PickupAddress Pickup_Address { get; set; }
        public DeliveryAddress Delivery_Address { get; set; }
    }
}
