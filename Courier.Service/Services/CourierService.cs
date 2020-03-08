using Courier.Service.Interfaces;
using Courier.Service.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class CourierService : ICourierService<CourierRequest>
    {
        readonly List<IObserver<CourierRequest>> subjects;

        public CourierService()
        {
            subjects = new List<IObserver<CourierRequest>>();
        }

        public IDisposable Subscribe(IObserver<CourierRequest> subject)
        {
            if (!subjects.Contains(subject))
            {
                subjects.Add(subject);
            }

            return new Unsubscriber(subjects, subject);
        }

        public Task Process(CourierRequest request)
        {
            foreach (var subject in subjects)
            {
                subject.OnNext(request);
            }

            return Task.CompletedTask;
        }

        public void OnError(Exception ex)
        {
            foreach (var subject in subjects)
            {
                subject.OnError(ex);
            }
        }
    }
}
