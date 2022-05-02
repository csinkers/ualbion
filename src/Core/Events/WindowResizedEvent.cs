using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:window_resized")]
public class WindowResizedEvent : EngineEvent
{
    public WindowResizedEvent(int width, int height) { Width = width; Height = height; }
    [EventPart("width")] public int Width { get; }
    [EventPart("height")] public int Height { get; }
}