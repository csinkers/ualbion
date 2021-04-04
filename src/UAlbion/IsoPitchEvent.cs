using UAlbion.Api;

namespace UAlbion
{
    [Event("iso_pitch")]
    public class IsoPitchEvent : Event, IVerboseEvent
    {
        public IsoPitchEvent(float delta) => Delta = delta;
        [EventPart("delta")] public float Delta { get; }
    }
}