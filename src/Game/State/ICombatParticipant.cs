using UAlbion.Formats.Assets.Inv;
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
    bool IsDead { get; }
    int ExperienceReward { get; }
    void TakeDamage(int amount);
    void Heal(int amount);
    void SetCombatPosition(int newTileIndex);
    // Returns monster inventory for Apres loot collection; null for party members.
    IInventory GetLoot();
}