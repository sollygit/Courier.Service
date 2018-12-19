using Courier.Service.Models;
using Courier.Service.Models.ParcelLabel;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface IParcelLabelService
    {
        Task<Consignment> Create(ParcelLabelRequest request, CourierDetails courierDetails);
        Task<string> GetStatus(string consignmentId, CourierDetails courierDetails);
        Task<byte[]> Download(string consignmentId, string username);
    }
}
