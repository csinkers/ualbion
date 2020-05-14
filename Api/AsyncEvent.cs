using System;

namespace UAlbion.Api
{
    public abstract class AsyncEvent : Event, IAsyncEvent
    {
        Action _completionCallback;

        public AsyncEvent CloneWithCallback(Action completionCallback)
        {
            var clone = Clone();
            clone._completionCallback = completionCallback;
            return clone;
        }

        protected abstract AsyncEvent Clone();

        public AsyncStatus AsyncStatus { get; private set; } = AsyncStatus.Unacknowledged;

        public void Acknowledge()
        {
            switch (AsyncStatus)
            {
                case AsyncStatus.Acknowledged: ApiUtil.Assert("Async events should only be acknowledged once!"); break;
                case AsyncStatus.Complete: ApiUtil.Assert("This event has already been completed!"); break;
                default: AsyncStatus = AsyncStatus.Acknowledged; break;
            }
        }

        public void Complete()
        {
            if (AsyncStatus == AsyncStatus.Complete)
                ApiUtil.Assert("Tried to complete a completed async event");

            bool wasAcknowledged = AsyncStatus == AsyncStatus.Acknowledged;
            AsyncStatus = AsyncStatus.Complete;

            if (wasAcknowledged) // If it wasn't acked then we can assume we're still running synchronously and can just return to the caller.
            {
                if (_completionCallback == null)
                    ApiUtil.Assert("Tried to complete an uninitialised async event");
                else
                    _completionCallback();
            }
        }
    }
}
