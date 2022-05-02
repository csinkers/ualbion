using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.ScriptEvents;

[Event("show_picture")] // USED IN SCRIPT (TODO: unify with show_pic)
public class ShowPictureEvent : Event
{
    public ShowPictureEvent(PictureId pictureId, int x, int y) { PictureId = pictureId; X = x; Y = y; }
    [EventPart("pictureId ")] public PictureId PictureId { get; }
    [EventPart("x ")] public int X { get; }
    [EventPart("y")] public int Y { get; }
}