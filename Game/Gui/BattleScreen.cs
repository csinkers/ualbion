using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    internal class BattleScreen : Component, IUiElement
    {
        public BattleScreen() : base(null) { }
        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
