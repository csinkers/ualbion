using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("prompt:item_quantity")]
public class ItemQuantityPromptEvent : Event, IQueryEvent<int>
{
    public ItemQuantityPromptEvent(StringId text, SpriteId icon, int iconSubId, int max, bool useTenths)
    {
        Text = text;
        Icon = icon;
        IconSubId = iconSubId;
        Max = max;
        UseTenths = useTenths;
    }

    [EventPart("text")] public StringId Text { get; }
    [EventPart("icon")] public SpriteId Icon { get; }
    [EventPart("iconSubId")] public int IconSubId { get; }
    [EventPart("max")] public int Max { get; }
    [EventPart("useTenths")] public bool UseTenths { get; }
}