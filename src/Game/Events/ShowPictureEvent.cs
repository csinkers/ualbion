using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("show_picture")]
    public class ShowPictureEvent : GameEvent
    {
        public ShowPictureEvent(PictureId pictureId, int x, int y) { PictureId = pictureId; X = x; Y = y; }
        [EventPart("pictureId ")] public PictureId PictureId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
