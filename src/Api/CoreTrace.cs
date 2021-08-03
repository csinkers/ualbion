using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using T = System.Diagnostics.Tracing;

namespace UAlbion.Api
{
    [EventSource(Name = "UAlbion-CoreTrace")]
    public class CoreTrace : EventSource
    {
        public static CoreTrace Log { get; } = new();

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

        [T.Event(1, Level = EventLevel.Informational)]
        public void Info(string category, string message, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
            => WriteEvent(1, category ?? "", message ?? "", file ?? "", member ?? "", line);

        [T.Event(2, Level = EventLevel.Warning)]
        public void Warning(string category, string message, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
            => WriteEvent(2, category ?? "", message ?? "", file ?? "", member ?? "", line);

        [T.Event(3, Level = EventLevel.Error)]
        public void Error(string category, string message, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
            => WriteEvent(3, category ?? "", message ?? "", file ?? "", member ?? "", line);

        [T.Event(4, Level = EventLevel.Critical)]
        public void Critical(string category, string message, [CallerFilePath] string file = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
            => WriteEvent(4, category ?? "", message ?? "", file ?? "", member ?? "", line);

        public void StartFrame(long frameCount, double deltaMicroseconds) => WriteEvent(5, frameCount, deltaMicroseconds);
        public void CollectedRenderables(string renderer, int layerCount, int renderableCount) => WriteEvent(6, renderer ?? "", layerCount, renderableCount);
        public void StartDebugGroup(string name) => WriteEvent(7, name ?? "");
        public void StopDebugGroup(string name) => WriteEvent(8, name ?? "");

        [T.Event(9, Level = EventLevel.Informational, Task = Tasks.Raise, Opcode = EventOpcode.Start)]
        public void StartRaise(long eventId, int nesting, string type, string details) => WriteEvent(9, eventId, nesting, type ?? "", details ?? "");

        [T.Event(10, Level = EventLevel.Informational, Task = Tasks.Raise, Opcode = EventOpcode.Stop)]
        public void StopRaise(long eventId, int nesting, string type, string details, int subscriberCount) => WriteEvent(10, eventId, nesting, type, details, subscriberCount);

        [T.Event(11, Level = EventLevel.Verbose, Task = Tasks.Raise, Opcode = Opcodes.StartVerbose)]
        public unsafe void StartRaiseVerbose(long eventId, int nesting, string type, string details)
        {
            type ??= "";
            details ??= "";

            fixed (char* typePtr = type)
            fixed (char* detailsPtr = details)
            {
                EventData* data = stackalloc EventData[4];
                data[0].DataPointer = (IntPtr)(&eventId);
                data[0].Size = sizeof(long);
                data[1].DataPointer = (IntPtr)(&nesting);
                data[1].Size = sizeof(int);
                data[2].DataPointer = (IntPtr)typePtr;
                data[2].Size = (type.Length + 1) * 2;
                data[3].DataPointer = (IntPtr)detailsPtr;
                data[3].Size = (details.Length + 1) * 2;
                WriteEventCore(11, 4, data);
            }
        } //=> WriteEvent(10, eventId, nesting, type ?? "", details ?? "", exchangeName ?? "");

        [T.Event(12, Level = EventLevel.Verbose, Task = Tasks.Raise, Opcode = Opcodes.StopVerbose)]
        public unsafe void StopRaiseVerbose(long eventId, int nesting, string type, string details, int subscriberCount)
        {
            type ??= "";
            details ??= "";

            fixed (char* typePtr = type)
            fixed (char* detailsPtr = details)
            {
                EventData* data = stackalloc EventData[5];
                data[0].DataPointer = (IntPtr)(&eventId);
                data[0].Size = sizeof(long);
                data[1].DataPointer = (IntPtr)(&nesting);
                data[1].Size = sizeof(int);
                data[2].DataPointer = (IntPtr)typePtr;
                data[2].Size = (type.Length + 1) * 2;
                data[3].DataPointer = (IntPtr)detailsPtr;
                data[3].Size = (details.Length + 1) * 2;
                data[4].DataPointer = (IntPtr)subscriberCount;
                data[4].Size = sizeof(int);
                WriteEventCore(12, 5, data);
            }
        } // => WriteEvent(11, eventId, nesting, type ?? "", details ?? "", exchangeName ?? "", subscriberCount);

        public void CreatedDeviceTexture(string name, int width, int height, int layers) => WriteEvent(12, name ?? "", width, height, layers);
        public void StartupEvent(string name) => WriteEvent(13, name ?? "");
        public void AssertFailed(string message) => WriteEvent(14, message ?? "");
    }
}
