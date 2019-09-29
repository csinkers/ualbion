using System;
using System.Diagnostics.Tracing;

namespace UAlbion.Core
{
    [EventSource(Name="UAlbion-CoreTrace")]
    class CoreTrace : EventSource
    {
        public static CoreTrace Log { get; } = new CoreTrace();

        [NonEvent]
        public void SetCorrelationId(Guid correlationId)
        {
            SetCurrentThreadActivityId(correlationId, out _);
        }

        [Event(1, Level = EventLevel.Informational)]
        public void Info(string category, string message)
        {
            WriteEvent(1, category, message);
        }

        [Event(2, Level = EventLevel.Warning)]
        public void Warning(string category, string message)
        {
            WriteEvent(2, category, message);
        }

        [Event(3, Level = EventLevel.Error)]
        public void Error(string category, string message)
        {
            WriteEvent(3, category, message);
        }

        public void StartFrame(long frameCount, double deltaMicroseconds)
        {
            WriteEvent(4, frameCount, deltaMicroseconds);
        }

        public void CollectedRenderables(string renderer, int layerCount, int renderableCount)
        {
            WriteEvent(5, renderer, layerCount, renderableCount);
        }

        public void StartDebugGroup(string name)
        {
            WriteEvent(6, name);
        }

        public void StopDebugGroup(string name)
        {
            WriteEvent(7, name);
        }

        [Event(8, Level = EventLevel.Informational, Opcode = EventOpcode.Start)]
        public void StartRaise(string type, string details)
        {
            WriteEvent(8, type, details);
        }

        [Event(9, Level = EventLevel.Informational, Opcode = EventOpcode.Stop)]
        public void StopRaise(string type, string details, int subscriberCount)
        {
            WriteEvent(9, type, details, subscriberCount);
        }

        [Event(10, Level = EventLevel.Verbose, Opcode = EventOpcode.Start)]
        public void StartRaiseVerbose(string type, string details)
        {
            WriteEvent(10, type, details);
        }

        [Event(11, Level = EventLevel.Verbose, Opcode = EventOpcode.Stop)]
        public void StopRaiseVerbose(string type, string details, int subscriberCount)
        {
            WriteEvent(11, type, details, subscriberCount);
        }

        public void CreatedDeviceTexture(string name, uint width, uint height, uint layers)
        {
            WriteEvent(12, name, width, height, layers);
        }
    }
}
