using System.Diagnostics.CodeAnalysis;

namespace UAlbion.Api.Eventing;

// Some of these interfaces are checked for extremely frequently and the performance cost of using attributes instead would be excessive.
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IEvent { void Format(IScriptBuilder builder); }
// ReSharper disable once UnusedTypeParameter


// IEvent - any event
// ISyncEvent - handlers cannot block
// IAsyncEvent - handlers can block
// IFirstResultEvent<TResult>
// IAllResultsEvent<TResult> - returns List<T>

public interface IAsyncEvent<T> : IEvent { }
public interface IAsyncEvent : IAsyncEvent<Unit> { }
public interface IHighlightEvent : IEvent { }
public interface IVerboseEvent : IEvent { }
public interface ICancellableEvent : IEvent { bool Propagating { get; set; } }
public interface IBranchingEvent : IAsyncEvent<bool> { }

public interface IEventNode
{
    ushort Id { get; }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Don't care about VB")]
    IEvent Event { get; }

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Don't care about VB")]
    IEventNode Next { get; }
    void Format(IScriptBuilder builder, int idOffset);
}

public interface IBranchNode : IEventNode
{
    IEventNode NextIfFalse { get; }
}
#pragma warning restore 1040 // Avoid empty interfaces