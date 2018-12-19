using Courier.Service.Models.Notification;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface INotificationService
    {
        Task<string> Send(NotificationRequest request, string username, string consignmentId);
    }
}
