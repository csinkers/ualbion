using System.Numerics;
using UAlbion.Game.Entities;

namespace UAlbion.Game.State
{
    internal class Player : IPlayer
    {
        public string Name { get; }

        public IInventory Inventory { get; }
        public Vector2 Position { get; }

        public ICharacterSheet Stats { get; }

        public int Age { get; }
        public bool IsMale { get; }
        public int CombatPosition { get; }

        public Item Head { get; }
        public Item Neck { get; }
        public Item LeftHand { get; }
        public Item RightHand { get; }
        public Item LeftRing { get; }
        public Item RightRing { get; }
        public Item Feet { get; }
        public Item Torso { get; }
    }
}
