using System;
using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        PartyCharacterId Id { get; }
        int CombatPosition { get; }
        IEffectiveCharacterSheet Effective { get; }
        IEffectiveCharacterSheet Apparent { get; }
        InventoryAction GetInventoryAction(ItemSlotId slotId);
        Func<Vector3> GetPosition { get; set; } // TODO: Find a better solution
    }
}