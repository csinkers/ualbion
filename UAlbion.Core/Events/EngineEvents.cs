using System;

namespace UAlbion.Core.Events
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

    [Event("e:projection_changed")] public class ProjectionMatrixChangedEvent : EngineEvent { }
    [Event("e:view_changed")] public class ViewMatrixChangedEvent : EngineEvent { }

    //[Event("e:render")]
    public class RenderEvent : EngineEvent
    {
        public RenderEvent(Action<IRenderable> add, Func<Type, IRenderer> getRenderer) { Add = add; GetRenderer = getRenderer; }
        public Action<IRenderable> Add { get; }
        public Func<Type, IRenderer> GetRenderer { get; }
    }

    [Event("e:toggle_fullscreen")] public class ToggleFullscreenEvent : EngineEvent { }
    [Event("e:load_renderdoc")] public class LoadRenderDocEvent : EngineEvent { }
    [Event("e:toggle_resizable")] public class ToggleResizableEvent : EngineEvent { }
    [Event("e:toggle_visible_border")] public class ToggleVisibleBorderEvent : EngineEvent { }
    [Event("quit", "Exit the game.", new[] { "exit" })] public class QuitEvent : EngineEvent { }
    [Event("e:key_down")] public class KeyDownEvent : EngineEvent { }
    [Event("e:scene_changed")] public class SceneChangedEvent : EngineEvent {
        public SceneChangedEvent(string sceneId) { SceneId = sceneId; }
        [EventPart("scene_id")] public string SceneId { get; }
    }
}
