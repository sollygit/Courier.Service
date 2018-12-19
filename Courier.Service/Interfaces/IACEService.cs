using Courier.Service.Models;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface IACEService
    {
        Task<string> UpdateParcelLabel(string locationId, string orderNumber, Consignment consignment, string username);
    }
}
