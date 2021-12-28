using System.Numerics;

namespace UAlbion.Game.Gui.Controls;

public class Spacing : UiElement, IFixedSizeUiElement
{
    readonly Vector2 _size;

    public Spacing(int x, int y)
    {
        _size = new Vector2(x, y);
    }
    public override Vector2 GetSize() => _size;
}