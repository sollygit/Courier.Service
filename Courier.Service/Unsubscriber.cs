using Courier.Service.Models;
using System;
using System.Collections.Generic;

namespace Courier.Service
{
    public class Unsubscriber : IDisposable
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
