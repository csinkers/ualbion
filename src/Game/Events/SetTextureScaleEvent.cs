using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("set_tex_scale")]
    public class SetTextureScaleEvent : GameEvent
    {
        [EventPart("x")] public float X { get; }
        [EventPart("y")] public float Y { get; }
        public SetTextureScaleEvent(float x, float y) { X = x; Y = y; }
    }
}
