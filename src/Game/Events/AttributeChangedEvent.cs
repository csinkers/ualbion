using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

public class AttributeChangedEvent : GameEvent, IVerboseEvent
{
    public SheetId SheetId { get; }
    public PhysicalAttribute Attribute { get; }

    public AttributeChangedEvent(SheetId sheetId, PhysicalAttribute attribute)
    {
        SheetId = sheetId;
        Attribute = attribute;
    }
}