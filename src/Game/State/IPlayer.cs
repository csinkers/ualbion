using System;
using System.Numerics;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State;

public interface IPlayer : ICombatParticipant
{
    PartyMemberId Id { get; }
    IEffectiveCharacterSheet Apparent { get; }
    // InventoryAction GetInventoryAction(ItemSlotId slotId);
    Vector3 GetPosition();
    void SetPositionFunc(Func<Vector3> func); // TODO: Refactor
    Vector2 StatusBarUiPosition { get; }
}