using System.Numerics;

namespace UAlbion.Game.Gui.Controls;

public class VariableSpacing : UiElement
{
    readonly Vector2 _size;
    public VariableSpacing(int x, int y) => _size = new Vector2(x, y);
    public override Vector2 GetSize() => _size;
}