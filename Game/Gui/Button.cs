using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui
{
    internal class Button : Component
    {
        static readonly Handler[] Handlers =
        {
            new Handler<Button, SubscribedEvent>((x, _) =>
            {
                x.Exchange.Attach(x._text);
            }), 
        };
        readonly Text _text;
        readonly Vector2 _position;
        int _width;
        int _height;

        public Button(Vector2 position, int w, int h, string text) : base(Handlers)
        {
            _position = position;
            _width = w;
            _height = h;
            _text = new Text(MetaFontId.FontColor.White, false, text, _position);
        }
    }
}
