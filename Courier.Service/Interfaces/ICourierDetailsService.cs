using Courier.Service.Models;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface ICourierDetailsService
    {
        Task<CourierDetails> Get(int locationId, string deliveryType);
    }
}
