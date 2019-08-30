using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class SetCameraPositionEvent : EngineEvent, IVerboseEvent
    {
        public SetCameraPositionEvent(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}