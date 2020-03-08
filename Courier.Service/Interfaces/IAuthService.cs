using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface IAuthService
    {
        Task<string> GetToken();
    }
}
