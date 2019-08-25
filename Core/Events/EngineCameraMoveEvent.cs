using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:camera_move", "Move the camera using relative coordinates.")]
    public class EngineCameraMoveEvent : EngineEvent, IVerboseEvent
    {
        public EngineCameraMoveEvent(float x, float y) { X = x; Y = y; }
        [EventPart("x ")] public float X { get; }
        [EventPart("y")] public float Y { get; }
    }
}