using System.Collections.Generic;
using PixelEngine;
using UAlbion.Gui;

namespace UAlbion.Entities
{
    class Item : Inanimate
    {
        readonly IList<PlayerClass> _validClasses = new List<PlayerClass>();
        Sprite _worldSprite;
        Sprite _inventorySprite;
        string _name;
        int _damage;
        int _protection;
        int _ammoType;
        int _weight; // Grams
        bool _isTwoHanded;
    }
}