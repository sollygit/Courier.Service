using Courier.Service.Models;
using Courier.Service.Models.ParcelPickup;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface IParcelPickupService
    {
        Task<string> ParcelPickup(ParcelPickupRequest request, CourierDetails courierDetails);
    }
}
