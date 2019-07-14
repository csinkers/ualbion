using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAlbion.Formats;

namespace UAlbion.Entities
{
    internal class Animate
    {
        (int,int) _position;
        (int,int) _goal;
        int _frame;
        AlbionSprite _sprite;
    }
}
