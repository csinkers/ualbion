using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:camera_move", "Move the camera using relative coordinates.")]
    public class EngineCameraMoveEvent : EngineEvent, IVerboseEvent
    {
        public EngineCameraMoveEvent(float x, float y, bool? absolute = null) { X = x; Y = y; Absolute = absolute; }
        [EventPart("x ")] public float X { get; }
        [EventPart("y")] public float Y { get; }
        [EventPart("absolute", true)] public bool? Absolute { get; }
    }
}