using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class InventoryButton : Component, IUiElement
    {
        public InventoryButton() : base(null) { }
        // Func<Item> _itemGetter;

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}