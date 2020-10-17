using System;
using System.Numerics;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        PartyMemberId Id { get; }
        int CombatPosition { get; }
        IEffectiveCharacterSheet Effective { get; }
        IEffectiveCharacterSheet Apparent { get; }
        // InventoryAction GetInventoryAction(ItemSlotId slotId);
        Func<Vector3> GetPosition { get; set; } // TODO: Find a better solution
        Vector2 StatusBarUiPosition { get; }
    }
}
