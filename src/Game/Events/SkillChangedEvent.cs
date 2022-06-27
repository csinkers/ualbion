using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

public class SkillChangedEvent : GameEvent, IVerboseEvent
{
    public SheetId SheetId { get; }
    public Skill Skill { get; }

    public SkillChangedEvent(SheetId sheetId, Skill skill)
    {
        SheetId = sheetId;
        Skill = skill;
    }
}