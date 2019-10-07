using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Divider : Component, IUiElement
    {
        readonly MetaFontId.FontColor _color;
        public Divider(MetaFontId.FontColor color) : base(null) { _color = color; }
        public Vector2 Size => new Vector2(0, 1);
        public void Render(Rectangle extents, Action<IRenderable> addFunc)
        {
            // TODO: Render line
        }
    }
}
