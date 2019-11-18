using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        PartyCharacterId Id { get; }
        Vector2 Position { get; }
        int CombatPosition { get; }
        IEffectiveCharacterSheet Effective { get; }
        IEffectiveCharacterSheet Apparent { get; }
        InventoryAction GetInventoryAction(ItemSlotId slotId);
    }
}