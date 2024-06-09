using System.Diagnostics.CodeAnalysis;

namespace UAlbion.Api.Eventing;

// Some of these interfaces are checked for extremely frequently and the performance cost of using attributes instead would be excessive.
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IEvent { void Format(IScriptBuilder builder); }
// ReSharper disable once UnusedTypeParameter
public interface IVerboseEvent : IEvent { } // Events which happen too often to show in the console
public interface ICancellableEvent : IEvent { bool Propagating { get; set; } } // Events where earlier handlers can stop later handlers from running.
public interface IQueryEvent : IEvent { } // Events where a single result is returned.

// ReSharper disable once UnusedTypeParameter
public interface IQueryEvent<TResult> : IQueryEvent { } // Events where a single result is returned.
public interface IBranchingEvent : IQueryEvent<bool> { }

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