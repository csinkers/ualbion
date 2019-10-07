using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Entities;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class InventoryButton : Component, IUiElement
    {
        public InventoryButton() : base(null) { }
        Func<Item> _itemGetter;
        public Vector2 Size { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}