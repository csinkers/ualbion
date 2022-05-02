using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("camera_lock", "Lock camera movement so it no longer follows the party.")] // USED IN SCRIPT
public class CameraLockEvent : Event
{
}