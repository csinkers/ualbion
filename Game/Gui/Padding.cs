using System.Numerics;

namespace UAlbion.Game.Gui
{
    public class Padding : UiElement
    {
        readonly Vector2 _size;
        public Padding(int x, int y) : base(null) { _size = new Vector2(x, y); }
        public override Vector2 GetSize() => _size;
    }
}