using System;

namespace UAlbion.Api
{
    public class AsyncEvent : Event, IAsyncEvent
    {
        bool _acknowledged;
        bool _complete;
        Action _completionCallback;

        public void SetCallback(Action completionCallback)
        {
            if (_completionCallback != null)
                throw new InvalidOperationException("Completion callback already set!");
            _completionCallback = completionCallback ?? throw new ArgumentNullException(nameof(completionCallback));
        }

        public bool Acknowledged
        {
            get => _acknowledged;
            set
            {
                if (!value)
                    throw new InvalidOperationException("Async events cannot be unacknowledged");

                if (_acknowledged)
                    throw new InvalidOperationException("Async events should only be acknowledged once!");

                _acknowledged = true;
            }
        }

        public void Complete()
        {
            if(_complete)
                throw new InvalidOperationException("Tried to complete a completed async event");

            if (_completionCallback == null)
                throw new InvalidOperationException("Tried to complete an uninitialised async event");
            _complete = true;
            _completionCallback();
        }
    }
}