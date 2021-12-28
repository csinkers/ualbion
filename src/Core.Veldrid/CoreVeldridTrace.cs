using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace UAlbion.Core.Veldrid;

[EventSource(Name = "UAlbion-CoreVeldridTrace")]
public class CoreVeldridTrace : EventSource
{
    public static CoreVeldridTrace Log { get; } = new();

    // The Tasks and Opcodes classes need to be created exactly as shown or the reflection
    // code in EventSource won't pick them up.
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
#pragma warning disable CA1724 // Conflict with namespace name
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public class Tasks
    {
        public const EventTask Raise = (EventTask)1;
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public class Opcodes
    {
        public const EventOpcode StartVerbose = (EventOpcode)10;
        public const EventOpcode StopVerbose = (EventOpcode)11;
    }
#pragma warning restore CA1724 // Conflict with namespace name
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
#pragma warning restore CA1034 // Nested types should not be visible

    [NonEvent]
    public static void SetCorrelationId(Guid correlationId) => SetCurrentThreadActivityId(correlationId, out _);

    [Event(1, Level = EventLevel.Informational)]
    public void Info(string category, string message, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        => WriteEvent(1, category ?? "", message ?? "", file ?? "", member ?? "", line);

}