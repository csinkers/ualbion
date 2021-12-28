using UAlbion.Api;

namespace UAlbion.Game.Events;

[Event("set_tex_offset")]
public class SetTextureOffsetEvent : GameEvent
{
    [EventPart("x")] public float X { get; }
    [EventPart("y")] public float Y { get; }
    public SetTextureOffsetEvent(float x, float y) { X = x; Y = y; }
}