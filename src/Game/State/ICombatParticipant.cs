using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State;

public interface ICombatParticipant
{
    int CombatPosition { get; }
    int X => CombatPosition % SavedGame.CombatColumns;
    int Y => CombatPosition / SavedGame.CombatColumns;
    SheetId SheetId { get; }
    SpriteId TacticalSpriteId { get; }
    SpriteId CombatSpriteId { get; }
    IEffectiveCharacterSheet Effective { get; }
}