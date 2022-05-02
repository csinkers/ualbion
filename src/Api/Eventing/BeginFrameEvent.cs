namespace UAlbion.Api.Eventing;

[Event("begin_frame", "Emitted at the beginning of each frame to allow components to clear any per-frame state.")]
public class BeginFrameEvent : Event, IVerboseEvent
{
    public static BeginFrameEvent Instance { get; } = new();
}