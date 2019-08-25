using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("show_picture")]
    public class ShowPictureEvent : GameEvent
    {
        public ShowPictureEvent(int pictureId, int x, int y) { PictureId = pictureId; X = x; Y = y; }
        [EventPart("pictureId ")] public int PictureId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}