using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

public class AttributeChangedEvent : GameEvent, IVerboseEvent
{
    public SheetId SheetId { get; }
    public Attribute Attribute { get; }

    public AttributeChangedEvent(SheetId sheetId, Attribute attribute)
    {
        SheetId = sheetId;
        Attribute = attribute;
    }
}