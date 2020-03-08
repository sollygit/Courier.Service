using Courier.Service.Interfaces;
using Courier.Service.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class EventBusService : IEventBusService<CourierRequest>
    {
        readonly List<IObserver<CourierRequest>> subjects;

        public EventBusService()
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

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<CourierRequest>> _subjects;
            private readonly IObserver<CourierRequest> _subject;

            public Unsubscriber(List<IObserver<CourierRequest>> subjects, IObserver<CourierRequest> subject)
            {
                _subjects = subjects;
                _subject = subject;
            }

            public void Dispose()
            {
                if (_subject != null)
                {
                    _subjects.Remove(_subject);
                }
            }
        }
    }
}
