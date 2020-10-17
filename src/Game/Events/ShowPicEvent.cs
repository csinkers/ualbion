using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("show_pic")]
    public class ShowPicEvent : GameEvent
    {
        public ShowPicEvent(SpriteId picId, int? x, int? y) { PicId = picId; X = x; Y = y; }
        [EventPart("picId ")] public SpriteId PicId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }
}
