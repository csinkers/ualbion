using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class Text : UiElement
    {
        enum Justification
        {
            Left,
            Center,
            Right
        }

        static readonly IDictionary<char, int> FontMapping = new Dictionary<char, int>
        {
            { 'a',  0 }, { 'b',  1 }, { 'c',  2 }, { 'd',  3 }, { 'e',  4 },
            { 'f',  5 }, { 'g',  6 }, { 'h',  7 }, { 'i',  8 }, { 'j',  9 },
            { 'k', 10 }, { 'l', 11 }, { 'm', 12 }, { 'n', 13 }, { 'o', 14 },
            { 'p', 15 }, { 'q', 16 }, { 'r', 17 }, { 's', 18 }, { 't', 19 },
            { 'u', 20 }, { 'v', 21 }, { 'w', 22 }, { 'x', 23 }, { 'y', 24 }, { 'z', 25 },
            { 'A', 26 }, { 'B', 27 }, { 'C', 28 }, { 'D', 29 }, { 'E', 30 },
            { 'F', 31 }, { 'G', 32 }, { 'H', 33 }, { 'I', 34 }, { 'J', 35 },
            { 'K', 36 }, { 'L', 37 }, { 'M', 38 }, { 'N', 39 }, { 'O', 40 },
            { 'P', 41 }, { 'Q', 42 }, { 'R', 43 }, { 'S', 44 }, { 'T', 45 },
            { 'U', 46 }, { 'V', 47 }, { 'W', 48 }, { 'X', 49 }, { 'Y', 50 }, { 'Z', 51 },
            { '1', 52 }, { '2', 53 }, { '3', 54 }, { '4', 55 }, { '5', 56 },
            { '6', 57 }, { '7', 58 }, { '8', 59 }, { '9', 60 }, { '0', 61 },
            { 'ä', 62 }, { 'Ä', 63 }, { 'ö', 64 }, { 'Ö', 65 }, { 'ü', 66 }, { 'Ü', 67 }, { 'ß', 68 },
            { '.', 69 }, { ':', 70 }, { ',', 71 }, { ';', 72 }, { '\'', 73 }, { '$', 74 }, // Weird H thingy?
            { '"', 75 }, { '?', 76 }, { '!', 77 }, { '/', 78 }, { '(', 79 }, { ')', 80 },
            { '#', 81 }, { '%', 82 }, { '*', 83 }, { '&', 84 }, { '+', 85 }, { '-', 86 },
            { '=', 87 }, { '>', 88 }, { '<', 89 }, { '^', 90 }, // Little skull / face glyph?
            { '♂', 91 }, { '♀', 92 }, // Male & female
            { 'é', 93 }, { 'â', 94 }, { 'à', 95 }, { 'ç', 96 }, { 'ê', 97 }, { 'ë', 98 }, { 'è', 99 },
            { 'ï', 100 }, { 'î', 101 }, { 'ì', 102 }, { 'ô', 103 }, { 'ò', 104 },
            { 'û', 105 }, { 'ù', 106 }, { 'á', 107 }, { 'í', 108 }, { 'ó', 109 }, { 'ú', 110 },
        };

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Text, WindowResizedEvent>((x,e) => x.Rebuild()),
            H<Text, SetLanguageEvent>((x,e) => x.Rebuild()),
            H<Text, SubscribedEvent>((x,e) => x.Rebuild())
        );

        // Driving properties
        readonly StringId _id;
        string _literal;
        bool _isBold;
        FontColor _color = FontColor.White;
        Justification _justification = Justification.Left;

        // Dependent properties
        UiMultiSprite _sprite;
        Vector2 _size;

        public Text(string literal) : base(Handlers) { _literal = literal; }
        public Text(StringId id) : base(Handlers) { _id = id; }

        public override string ToString()
        {
            var assets = Exchange?.Resolve<IAssetManager>();
            var settings = Exchange?.Resolve<ISettings>();
            if(assets == null || settings == null)
                return $"Text:{_id}";

            var text = _literal ?? assets.LoadString(_id, settings.Language);
            return $"Text:\"{text}\" {_color} {(_isBold ? "Bold" : "")} {_justification} ({_size.X}x{_size.Y})";
        }

        void Rebuild()
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var settings = Resolve<ISettings>();

            var font = assets.LoadFont(_color, false);
            var text = _literal ?? assets.LoadString(_id, settings.Language);

            var instances = new SpriteInstanceData[text.Length * (_isBold ? 4 : 2)];
            int offset = 0;
            var flags = SpriteFlags.UsePalette | SpriteFlags.NoTransform | SpriteFlags.NoDepthTest | SpriteFlags.LeftAligned;

            for (int i = 0; i < text.Length; i++)
            {
                int n = i * (_isBold ? 4 : 2);
                char c = text[i];
                if (FontMapping.TryGetValue(c, out var index))
                {
                    font.GetSubImageDetails(index, out var size, out var texOffset, out var texSize, out var layer);

                    // Adjust texture coordinates slightly to avoid bleeding
                    texOffset.Y += 0.1f / font.Height;

                    var normPosition = window.UiToNormRelative(new Vector2(offset, 0));
                    var baseInstance = new SpriteInstanceData(
                        new Vector3(normPosition, 0),
                        window.UiToNormRelative(new Vector2(size.X, size.Y)),
                        texOffset, texSize, layer, flags);

                    instances[n] = baseInstance;
                    instances[n+1] = baseInstance;
                    if(_isBold)
                    {
                        instances[n + 2] = baseInstance;
                        instances[n + 3] = baseInstance;

                        instances[n].Offset += new Vector3(window.UiToNormRelative(new Vector2(2, 1)), 0);
                        instances[n].Flags |= SpriteFlags.DropShadow;

                        instances[n+1].Offset += new Vector3(window.UiToNormRelative(new Vector2(1,1)), 0);
                        instances[n+1].Flags |= SpriteFlags.DropShadow;

                        instances[n + 2].Offset += new Vector3(window.UiToNormRelative(new Vector2(1, 0)), 0);
                        offset += 1;
                    }
                    else
                    {
                        instances[n].Flags |= SpriteFlags.DropShadow;
                        instances[n].Offset += new Vector3(window.UiToNormRelative(new Vector2(1,1)), 0);
                    }

                    offset += (int)size.X;
                }
                else
                {
                    offset += 3;
                }
            }

            _sprite = new UiMultiSprite(new SpriteKey(font, (int)DrawLayer.Interface, flags)) { Instances = instances, };
            font.GetSubImageDetails(0, out var fontSize, out _, out _, out _);
            _size = new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
        }

        public Text Bold() { _isBold = true; return this; }
        public Text Color(FontColor color) { _color = color; return this; }
        public Text Left() { _justification = Justification.Left; return this; }
        public Text Center() { _justification = Justification.Center; return this; }
        public Text Right() { _justification = Justification.Right; return this; }
        public Text LiteralString(string literal)
        {
            if (_literal != literal)
            {
                _literal = literal;
                Rebuild();
            }

            return this;
        }

        public override Vector2 GetSize() => _size;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var window = Resolve<IWindowManager>();
            if (_sprite.RenderOrder != order)
                _sprite.RenderOrder = order;

            var newPosition = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            switch (_justification)
            {
                case Justification.Left:
                    break;
                case Justification.Center:
                    newPosition += 
                        new Vector3(
                        window.UiToNormRelative(new Vector2(
                            (extents.Width - _size.X) / 2,
                            (extents.Height - _size.Y) / 2)), 
                        0);
                    break;
                case Justification.Right:
                    newPosition += 
                        new Vector3(
                        window.UiToNormRelative(new Vector2(
                            extents.Width - _size.X,
                            extents.Height - _size.Y)), 
                        0);
                    break;
            }

            if (_sprite.Position != newPosition) // Check first to avoid excessive triggering of the ExtentsChanged event.
                _sprite.Position = newPosition;
            addFunc(_sprite);
            return order;
        }
    }
}
