using System;
using System.Diagnostics.Tracing;

namespace UAlbion.Api
{
    [EventSource(Name = "UAlbion-CoreTrace")]
    public class CoreTrace : EventSource
    {
        public static CoreTrace Log { get; } = new CoreTrace();

        public class Tasks
        {
            public const EventTask Raise = (EventTask)1;
        }

        public class Opcodes
        {
            public const EventOpcode StartVerbose = (EventOpcode)10;
            public const EventOpcode StopVerbose = (EventOpcode)11;
        }

        [NonEvent]
        public void SetCorrelationId(Guid correlationId) => SetCurrentThreadActivityId(correlationId, out _);

        [System.Diagnostics.Tracing.Event(1, Level = EventLevel.Informational)]
        public void Info(string category, string message) => WriteEvent(1, category ?? "", message ?? "");

        [System.Diagnostics.Tracing.Event(2, Level = EventLevel.Warning)]
        public void Warning(string category, string message) => WriteEvent(2, category ?? "", message ?? "");

        [System.Diagnostics.Tracing.Event(3, Level = EventLevel.Error)]
        public void Error(string category, string message) => WriteEvent(3, category ?? "", message ?? "");

        public void StartFrame(long frameCount, double deltaMicroseconds) => WriteEvent(4, frameCount, deltaMicroseconds);
        public void CollectedRenderables(string renderer, int layerCount, int renderableCount) => WriteEvent(5, renderer ?? "", layerCount, renderableCount);
        public void StartDebugGroup(string name) => WriteEvent(6, name ?? "");
        public void StopDebugGroup(string name) => WriteEvent(7, name ?? "");

        [System.Diagnostics.Tracing.Event(8, Level = EventLevel.Informational, Task = Tasks.Raise, Opcode = EventOpcode.Start)]
        public void StartRaise(long eventId, int nesting, string type, string details, string exchangeName) => WriteEvent(8, eventId, nesting, type ?? "", details ?? "", exchangeName ?? "");

        [System.Diagnostics.Tracing.Event(9, Level = EventLevel.Informational, Task = Tasks.Raise, Opcode = EventOpcode.Stop)]
        public void StopRaise(long eventId, int nesting, string type, string details, string exchangeName, int subscriberCount) => WriteEvent(9, eventId, nesting, type, details, exchangeName ?? "", subscriberCount);

        [System.Diagnostics.Tracing.Event(10, Level = EventLevel.Verbose, Task = Tasks.Raise, Opcode = Opcodes.StartVerbose)]
        public unsafe void StartRaiseVerbose(long eventId, int nesting, string type, string details, string exchangeName)
        {
            type ??= "";
            details ??= "";
            exchangeName ??= "";

            fixed (char* typePtr = type)
            fixed (char* detailsPtr = details)
            fixed (char* exchangePtr = exchangeName)
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
                data[4].DataPointer = (IntPtr)exchangePtr;
                data[4].Size = (exchangeName.Length + 1) * 2;
                WriteEventCore(10, 5, data);
            }
        } //=> WriteEvent(10, eventId, nesting, type ?? "", details ?? "", exchangeName ?? "");

        [System.Diagnostics.Tracing.Event(11, Level = EventLevel.Verbose, Task = Tasks.Raise, Opcode = Opcodes.StopVerbose)]
        public unsafe void StopRaiseVerbose(long eventId, int nesting, string type, string details, string exchangeName, int subscriberCount)
        {
            type ??= "";
            details ??= "";
            exchangeName ??= "";

            fixed (char* typePtr = type)
            fixed (char* detailsPtr = details)
            fixed (char* exchangePtr = exchangeName)
            {
                EventData* data = stackalloc EventData[6];
                data[0].DataPointer = (IntPtr)(&eventId);
                data[0].Size = sizeof(long);
                data[1].DataPointer = (IntPtr)(&nesting);
                data[1].Size = sizeof(int);
                data[2].DataPointer = (IntPtr)typePtr;
                data[2].Size = (type.Length + 1) * 2;
                data[3].DataPointer = (IntPtr)detailsPtr;
                data[3].Size = (details.Length + 1) * 2;
                data[4].DataPointer = (IntPtr)exchangePtr;
                data[4].Size = (exchangeName.Length + 1) * 2;
                data[5].DataPointer = (IntPtr)subscriberCount;
                data[5].Size = sizeof(int);
                WriteEventCore(11, 6, data);
            }
        } // => WriteEvent(11, eventId, nesting, type ?? "", details ?? "", exchangeName ?? "", subscriberCount);
        public void CreatedDeviceTexture(string name, uint width, uint height, uint layers) => WriteEvent(12, name ?? "", width, height, layers);
        public void StartupEvent(string name) => WriteEvent(13, name ?? "");
    }
}
