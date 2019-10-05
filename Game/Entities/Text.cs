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
    public class Text : Component, IUiElement
    {
        enum Justification
        {
            Left,
            Center,
            Right
        }

        readonly IDictionary<char, int> _fontMapping = new Dictionary<char, int>
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

        readonly StringId _id;
        MultiSprite _sprite;

        static readonly Handler[] Handlers =
        {
            new Handler<Text, WindowResizedEvent>((x,e) => x.Reformat()),
            new Handler<Text, SetLanguageEvent>((x,e) => x.Reformat()),
            new Handler<Text, SubscribedEvent>((x,e) => x.Reformat()),
        };

        MetaFontId.FontColor _color;
        bool _isBold;
        Justification _justification;

        void Render(RenderEvent renderEvent)
        {
            renderEvent.Add(_sprite);
        }

        void Reformat()
        {
            var assets = Exchange.Resolve<IAssetManager>();
            var window = Exchange.Resolve<IWindowState>();
            var settings = Exchange.Resolve<ISettings>();

            var font = assets.LoadFont(_color, false);
            var text = assets.LoadString(_id, settings.Language);

            font.GetSubImageDetails(0, out var size, out _, out var texSize, out _);
            var instances = new SpriteInstanceData[text.Length * (_isBold ? 4 : 2)];
            int offset = 0;
            for (int i = 0; i < text.Length; i++)
            {
                int n = i * (_isBold ? 4 : 2);
                char c = text[i];
                if (_fontMapping.TryGetValue(c, out var index))
                {
                    font.GetSubImageDetails(index, out size, out var texOffset, out texSize, out var layer);
                    size = new Vector2(size.X + 1, -size.Y);
                    var baseInstance = new SpriteInstanceData(
                        new Vector3(window.UiToScreenRelative(offset, 0), 0),
                        window.GuiScale * size / window.Size,
                        texOffset, texSize, layer,
                        SpriteFlags.UsePalette | SpriteFlags.NoTransform);

                    instances[n] = baseInstance;
                    instances[n+1] = baseInstance;
                    if(_isBold)
                    {
                        instances[n + 2] = baseInstance;
                        instances[n + 3] = baseInstance;

                        instances[n].Offset += new Vector3(window.UiToScreenRelative(2, 1), 0);
                        instances[n].Flags |= SpriteFlags.DropShadow;

                        instances[n+1].Offset += new Vector3(window.UiToScreenRelative(1,1), 0);
                        instances[n+1].Flags |= SpriteFlags.DropShadow;

                        instances[n + 2].Offset += new Vector3(window.GuiScale / window.Size.X, 0, 0);
                        offset += 1;
                    }
                    else
                    {
                        instances[n].Flags |= SpriteFlags.DropShadow;
                        instances[n].Offset += new Vector3(window.UiToScreenRelative(1,1), 0);
                    }

                    offset += (int)size.X;
                }
                else
                {
                    offset += 6;
                }
            }

            _sprite = new MultiSprite(new SpriteKey(font, (int)DrawLayer.Interface, false))
            {
                Instances = instances
            };
        }

        public Text(StringId id) : base(Handlers)
        {
            // TODO: Left & right justification, kerning, line wrapping
            _id = id;
            _color = MetaFontId.FontColor.White;
            _isBold = false;
            _justification = Justification.Left;
        }

        public Text Bold() { _isBold = true; return this; }
        public Text Color(MetaFontId.FontColor color) { _color = color; return this; }
        public Text Left() { _justification = Justification.Left; return this; }
        public Text Center() { _justification = Justification.Center; return this; }
        public Text Right() { _justification = Justification.Right; return this; }

        public IUiElement Parent { get; }
        public IList<IUiElement> Children => null;
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            _sprite.Position = new Vector3(position.X, position.Y, 0.0f); // TODO
            addFunc(_sprite);
        }
    }
}
