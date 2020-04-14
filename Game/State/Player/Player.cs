using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.State.Player
{
    public class Player : Component, IPlayer
    {
        readonly PlayerInventoryManager _inventoryManager;

        static readonly HandlerSet Handlers = new HandlerSet(
        );

        public Player(PartyCharacterId id, CharacterSheet sheet) : base(Handlers)
        {
            Id = id;
            _inventoryManager = AttachChild(new PlayerInventoryManager(id, sheet));
        }

        public PartyCharacterId Id { get; }
        public int CombatPosition { get; set; }
        public IEffectiveCharacterSheet Effective => _inventoryManager.Effective;
        public IEffectiveCharacterSheet Apparent => _inventoryManager.Apparent;
        public InventoryAction GetInventoryAction(ItemSlotId slotId) => _inventoryManager.GetInventoryAction(slotId);
        public Func<Vector3> GetPosition { get; set; }
        public override string ToString() => $"Player {Id}";

        public bool TryChangeInventory(ItemId itemId, QuantityChangeOperation operation, int amount)
            => _inventoryManager.TryChangeInventory(itemId, operation, amount);

        public bool TryChangeGold(QuantityChangeOperation operation, int amount)
            => _inventoryManager.TryChangeGold(operation, amount);


        public bool TryChangeRations(QuantityChangeOperation operation, int amount)
            => _inventoryManager.TryChangeRations(operation, amount);
    }
}

