using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("set_held_item_cursor", "Change the currently active held item associated with the mouse cursor")]
public class SetHeldItemCursorEvent : Event, IVerboseEvent
{
    [EventPart("sprite")] public SpriteId Sprite { get; }
    [EventPart("subItem", true, 0)] public int SubItem { get; }
    [EventPart("frames", true, 1)] public int FrameCount { get; }
    [EventPart("count", true, 1)] public int ItemCount { get; }
    [EventPart("tenths", true, false)] public bool UseTenths { get; }

    public SetHeldItemCursorEvent(SpriteId sprite, int subItem, int frameCount, int itemCount, bool useTenths)
    {
        Sprite = sprite;
        SubItem = subItem;
        FrameCount = frameCount;
        ItemCount = itemCount;
        UseTenths = useTenths;
    }

}