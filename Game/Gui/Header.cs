using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    class Header : Component
    {
        static readonly Handler[] Handlers =
        {
            new Handler<Header, SubscribedEvent>((x, _) =>
            {
                x.Exchange.Attach(x._text);
            }), 
        };

        readonly Text _text;
        readonly Vector2 _position;
        int _width;
        int _height;

        public Header(Vector2 position, int w, int h, StringId id) : base(Handlers)
        {
            _position = position;
            _width = w;
            _height = h;
            _text = new Text(id).Bold();
        }
    }
}