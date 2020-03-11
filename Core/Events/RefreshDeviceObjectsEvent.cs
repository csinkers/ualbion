using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:refresh_objects", "Refresh the graphics device objects")]
    public class RefreshDeviceObjectsEvent : EngineEvent
    {
        public RefreshDeviceObjectsEvent(int? count) { Count = count; }

        [EventPart("n", "Number of times to refresh")]
        public int? Count { get; }
    }
}
