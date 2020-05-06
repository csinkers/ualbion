using System;

namespace UAlbion.Api
{
    public abstract class AsyncEvent : Event, IAsyncEvent
    {
        bool _acknowledged;
        bool _complete;
        Action _completionCallback;

        public AsyncEvent CloneWithCallback(Action completionCallback)
        {
            var clone = Clone();
            clone._completionCallback = completionCallback;
            return clone;
        }

        protected abstract AsyncEvent Clone();

        public bool Acknowledged
        {
            get => _acknowledged;
            set
            {
                if (!value)
                    ApiUtil.Assert("Async events cannot be unacknowledged");

                if (_acknowledged)
                    ApiUtil.Assert("Async events should only be acknowledged once!");

                _acknowledged = true;
            }
        }

        public void Complete()
        {
            if (_complete)
                ApiUtil.Assert("Tried to complete a completed async event");

            _complete = true;

            if (_completionCallback == null)
                ApiUtil.Assert("Tried to complete an uninitialised async event");
            else
                _completionCallback();
        }
    }
}
