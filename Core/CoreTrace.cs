using System;
using System.Diagnostics.Tracing;

namespace UAlbion.Core
{
    class CoreTrace : EventSource
    {
        public static CoreTrace Log { get; } = new CoreTrace();

        [NonEvent]
        public void SetCorrelationId(Guid correlationId)
        {
            SetCurrentThreadActivityId(correlationId, out _);
        }

        public void Info(string category, string message)
        {
            WriteEvent(1, category, message);
        }

        public void Warning(string category, string message)
        {
            WriteEvent(2, category, message);
        }

        public void Error(string category, string message)
        {
            WriteEvent(3, category, message);
        }

        public void StartFrame(long frameCount, double deltaMicroseconds)
        {
            WriteEvent(4, frameCount, deltaMicroseconds);
        }

        public void CollectedRenderables(string renderer, int count)
        {
            WriteEvent(5, renderer, count);
        }

        public void StartDebugGroup(string name)
        {
            WriteEvent(6, name);
        }

        public void StopDebugGroup(string name)
        {
            WriteEvent(7, name);
        }

        public void StartRaise(string type, string details)
        {
            WriteEvent(8, type, details);
        }

        public void StopRaise(string type, string details, int subscriberCount)
        {
            WriteEvent(9, type, details, subscriberCount);
        }

        public void CreatedDeviceTexture(string name, uint width, uint height, uint layers)
        {
            WriteEvent(10, name, width, height, layers);
        }
    }
}
