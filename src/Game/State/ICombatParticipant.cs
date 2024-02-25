using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State;

public interface ICombatParticipant
{
    int CombatPosition { get; }
    SheetId SheetId { get; }
    SpriteId TacticalSpriteId { get; }
    SpriteId CombatSpriteId { get; }
    IEffectiveCharacterSheet Effective { get; }
}