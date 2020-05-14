namespace UAlbion.Api
{
    public interface IAsyncEvent : IEvent
    {
        /// <summary>
        /// Gets the current status of the async event
        /// </summary>
        AsyncStatus AsyncStatus { get; }
        /// <summary>
        /// Acknowledge the event so calling code knows the callback can be expected to be called.
        /// </summary>
        void Acknowledge(); // Call to indicate that the async event has been processed and the callback will be called in due time.
        /// <summary>
        /// Indicate that the async event has finished being processed, and further event execution should resume.
        /// </summary>
        void Complete();
    }
}
