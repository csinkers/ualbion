using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("show_pic")]
    public class ShowPicEvent : GameEvent
    {
        public ShowPicEvent(int picId, int? x, int? y) { PicId = picId; X = x; Y = y; }
        [EventPart("picId ")] public int PicId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }
}