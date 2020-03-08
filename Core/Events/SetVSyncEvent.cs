using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:set_vsync", "Enables or disables VSync")]
    public class SetVSyncEvent : EngineEvent
    {
        public SetVSyncEvent(bool value) { Value = value; }
        [EventPart("value")] public bool Value { get; }
    }
}