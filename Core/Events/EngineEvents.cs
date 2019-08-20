using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Events
{
    public interface IEngineEvent : IEvent { }
    public abstract class EngineEvent : Event, IEngineEvent { }

    [Event("e:update")]
    public class EngineUpdateEvent : EngineEvent, IVerboseEvent
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

    //[Event("e:render")]
    public class RenderEvent : EngineEvent, IVerboseEvent
    {
        public RenderEvent(Action<IRenderable> add) { Add = add; }
        public Action<IRenderable> Add { get; }
    }

    [Event("e:toggle_fullscreen")] public class ToggleFullscreenEvent : EngineEvent { }
    [Event("e:load_renderdoc")] public class LoadRenderDocEvent : EngineEvent { }
    [Event("e:toggle_resizable")] public class ToggleResizableEvent : EngineEvent { }
    [Event("e:toggle_visible_border")] public class ToggleVisibleBorderEvent : EngineEvent { }
    [Event("quit", "Exit the game.", new[] { "exit" })] public class QuitEvent : EngineEvent { }

    public class InputEvent : EngineEvent, IVerboseEvent
    {
        public double DeltaSeconds { get; }
        public InputSnapshot Snapshot { get; }
        public Vector2 MouseDelta { get; }

        public InputEvent(double deltaSeconds, InputSnapshot snapshot, Vector2 mouseDelta)
        {
            DeltaSeconds = deltaSeconds;
            Snapshot = snapshot;
            MouseDelta = mouseDelta;
        }
    }

    [Event("e:camera_move", "Move the camera using relative coordinates.")]
    public class EngineCameraMoveEvent : EngineEvent, IVerboseEvent
    {
        public EngineCameraMoveEvent(float x, float y) { X = x; Y = y; }
        [EventPart("x ")] public float X { get; }
        [EventPart("y")] public float Y { get; }
    }

    [Event("e:begin_frame", "Emitted at the beginning of each frame to allow components to clear any per-frame state.")]
    public class BeginFrameEvent : EngineEvent, IVerboseEvent { }

    [Event("e:subscribed", "Emitted to an object immediately after it is subscribed.")]
    public class SubscribedEvent : EngineEvent { }

    [Event("e:mag", "Changes the current magnification level.")]
    public class MagnifyEvent : EngineEvent
    {
        public MagnifyEvent(int delta)
        {
            Delta = delta;
        }

        [EventPart("delta", "The change in magnification level")]
        public int Delta { get; }

    }

    public class Selection
    {
        public Selection(Vector3 intersectionPoint, string name, object target)
        {
            IntersectionPoint = intersectionPoint;
            Name = name;
            Target = target;
        }

        public Vector3 IntersectionPoint { get; }
        public string Name { get; }
        public object Target { get; }
    }

    public class ShowDebugInfoEvent : Event, IVerboseEvent
    {
        public ShowDebugInfoEvent(IList<Selection> selections)
        {
            Selections = selections;
        }

        public IList<Selection> Selections { get; }
    }

    public class ScreenCoordinateSelectEvent : EngineEvent, IVerboseEvent
    {
        public ScreenCoordinateSelectEvent(Vector2 position, Action<float, Selection> registerHit)
        {
            Position = position;
            RegisterHit = registerHit;
        }

        public Vector2 Position { get; }
        public Action<float, Selection> RegisterHit { get; }
    }

    public class WorldCoordinateSelectEvent : EngineEvent, IVerboseEvent
    {
        readonly Action<float, Selection> _registerHit;

        public WorldCoordinateSelectEvent(Vector3 origin, Vector3 direction, Action<float, Selection> registerHit)
        {
            Origin = origin;
            Direction = direction;
            _registerHit = registerHit;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }

        public void RegisterHit(float t, string name, object target) => 
            _registerHit(t, new Selection(Origin + t * Direction, name, target));
    }

    public class SetRawPaletteEvent : EngineEvent, IVerboseEvent
    {
        public string Name { get; }
        public uint[] Entries { get; }

        public SetRawPaletteEvent(string name, uint[] entries)
        {
            Name = name;
            Entries = entries;
        }
    }

    public class PersistToDiskEvent : EngineEvent
    {
    }
}
