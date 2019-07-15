using System.Collections.Generic;
using UAlbion.Formats;

namespace UAlbion.Game.Entities
{
    class Item : Inanimate
    {
        readonly IList<PlayerClass> _validClasses = new List<PlayerClass>();
        AlbionSprite _worldSprite;
        AlbionSprite _inventorySprite;
        string _name;
        int _damage;
        int _protection;
        int _ammoType;
        int _weight; // Grams
        bool _isTwoHanded;
    }
}