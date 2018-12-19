using System;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface IEventBus<T> : IObservable<T>
    {
        Task Process(T request);
    }
}
