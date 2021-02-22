using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("camera_unlock", "Unlock camera movement so it resumes following the party.", "cu")] // USED IN SCRIPT
    public class CameraUnlockEvent : GameEvent
    {
    }
}
