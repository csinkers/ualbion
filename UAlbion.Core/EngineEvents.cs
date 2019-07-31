namespace UAlbion.Core
{
    public interface IEngineEvent : IEvent { }
    public abstract class EngineEvent : Event, IEngineEvent { }

    [Event("e:update")]
    public class EngineUpdateEvent : EngineEvent
    {
        public EngineUpdateEvent(float deltaSeconds) { DeltaSeconds = deltaSeconds; }
        [EventPart("delta_seconds")] public float DeltaSeconds { get; }
    }

    [Event("e:window_resized")]
    public class WindowResizedEvent : EngineEvent
    {
        public WindowResizedEvent(int width, int height) { Width = width; Height = height; } 
        [EventPart("width")] public int Width { get; }
        [EventPart("height")] public int Height { get; }
    }

    [Event("e:projection_changed")]
    public class ProjectionMatrixChangedEvent : EngineEvent
    {
    }

    [Event("e:view_changed")]
    public class ViewMatrixChangedEvent : EngineEvent
    {
    }

    [Event("e:render")]
    public class RenderEvent : EngineEvent
    {

    }
}
