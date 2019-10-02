using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    class CombatPositionsDialog : IUiElement
    {
        readonly InventoryButton[] _buttons = new InventoryButton[12];
        Button _okButton;
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