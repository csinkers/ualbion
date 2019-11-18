using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class Player : Component, IPlayer
    {
        readonly PlayerInventoryManager _inventoryManager;
        readonly CharacterSheet _base;

        public Player(PartyCharacterId id, CharacterSheet sheet, InventoryScreenState inventoryScreenState)
        {
            Id = id;
            _base = sheet;
            _inventoryManager = new PlayerInventoryManager(id, _base, inventoryScreenState);
        }

        public PartyCharacterId Id { get; }
        public Vector2 Position { get; set; }
        public int CombatPosition { get; set; }
        public IEffectiveCharacterSheet Effective => _inventoryManager.Effective;
        public IEffectiveCharacterSheet Apparent => _inventoryManager.Apparent;
        public InventoryAction GetInventoryAction(ItemSlotId slotId) => _inventoryManager.GetInventoryAction(slotId);
    }
}

