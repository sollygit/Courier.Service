using System;
using System.Threading.Tasks;

namespace Courier.Service.Interfaces
{
    public interface ICourierService<T> : IObservable<T>
    {
        Task Process(T request);
    }
}
