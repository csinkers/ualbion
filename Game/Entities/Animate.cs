using UAlbion.Formats;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Entities
{
    internal class Animate
    {
        (int,int) _position;
        (int,int) _goal;
        int _frame;
        AlbionSprite _sprite;
    }
}
