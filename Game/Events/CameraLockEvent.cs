using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("camera_lock", "Lock camera movement so it no longer follows the party.")]
    public class CameraLockEvent : GameEvent
    {
    }
}
