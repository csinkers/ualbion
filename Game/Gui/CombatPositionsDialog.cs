using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class CombatPositionsDialog : Component, IUiElement
    {
        public CombatPositionsDialog() : base(null) { }
        // readonly InventoryButton[] _buttons = new InventoryButton[12];
        // Button _okButton;
        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}