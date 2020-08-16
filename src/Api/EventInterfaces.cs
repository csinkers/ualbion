namespace UAlbion.Api
{
    // Some of these interfaces are checked for extremely frequently and the performance cost of using attributes instead would be excessive.
#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IEvent { }
    public interface IAsyncEvent : IEvent { }
    public interface IAsyncEvent<T> : IAsyncEvent { }
    public interface IHighlightEvent { }
    public interface IVerboseEvent : IEvent { }
    public interface ICancellableEvent : IEvent { bool Propagating { get; set; } }
#pragma warning restore 1040 // Avoid empty interfaces
}
