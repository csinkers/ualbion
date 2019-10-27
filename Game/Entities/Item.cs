using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities
{
    public class Item
    {
        public string Name { get; }
        public int Damage { get; }
        public int Protection { get; }
        public IconGraphicsId Icon { get; }
        public int AmmoType { get; }
        public int Weight { get; } // Grams
        public bool IsTwoHanded { get; }
        public IReadOnlyList<PlayerClass> ValidClasses { get; }
    }
}