using System;
using System.Numerics;

namespace UAlbion.Game.Gui;

public class SelectionContext
{
    public SelectionContext(Vector2 uiPosition, IUiElement.RegisterHitFunc hitFunc)
    {
        UiPosition = uiPosition;
        HitFunc = hitFunc ?? throw new ArgumentNullException(nameof(hitFunc));
    }

    public Vector2 UiPosition { get; }
    public IUiElement.RegisterHitFunc HitFunc { get; }
}