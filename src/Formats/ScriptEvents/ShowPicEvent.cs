using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.ScriptEvents;

[Event("show_pic")] // USED IN SCRIPT (TODO: unify with show_picture)
public class ShowPicEvent : Event
{
    public ShowPicEvent(PictureId picId, int? x, int? y) { PicId = picId; X = x; Y = y; }
    [EventPart("picId ")] public PictureId PicId { get; }
    [EventPart("x", true)] public int? X { get; }
    [EventPart("y", true)] public int? Y { get; }
}