using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("set_held_item_cursor", "Change the currently active held item associated with the mouse cursor")]
public class SetHeldItemCursorEvent : Event, IVerboseEvent
{
    [EventPart("sprite")] public SpriteId Sprite { get; }
    [EventPart("subItem")] public int SubItem { get; }
    [EventPart("frames")] public int FrameCount { get; }

    public SetHeldItemCursorEvent(SpriteId sprite, int subItem, int frameCount)
    {
        Sprite = sprite;
        SubItem = subItem;
        FrameCount = frameCount;
    }
}