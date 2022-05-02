using System.Diagnostics.CodeAnalysis;

namespace UAlbion.Api.Eventing;
// Some of these interfaces are checked for extremely frequently and the performance cost of using attributes instead would be excessive.
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IEvent { string ToStringNumeric(); }
public interface IAsyncEvent : IEvent { }
// ReSharper disable once UnusedTypeParameter
public interface IAsyncEvent<T> : IEvent { }
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
    string ToString(int idOffset);
}
public interface IBranchNode : IEventNode
{
    IEventNode NextIfFalse { get; }
}
#pragma warning restore 1040 // Avoid empty interfaces