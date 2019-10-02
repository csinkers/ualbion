using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class InventoryButton : IUiElement
    {
        Func<Item> _itemGetter;
        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Vector2 position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}