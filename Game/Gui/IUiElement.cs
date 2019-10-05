using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public interface IUiElement
    {
        IUiElement Parent { get; }
        IList<IUiElement> Children { get; }
        Vector2 Size { get; }
        bool FixedSize { get; }
        void Render(Rectangle position, Action<IRenderable> addFunc);
    }

    /*
    Arrangers:
        Vertical list (all widths set to max required width, all heights independently set based on required heights)
        Hardcoded
        Hardcoded width, height based on required height.
        Grid of fixed size elements
     */
}
