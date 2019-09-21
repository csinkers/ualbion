using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class Text : Component
    {
        readonly IDictionary<char, int> FontMapping = new Dictionary<char, int>
        {
            { 'a', 0 }, { 'b', 1 }, { 'c', 2 }, { 'd', 3 }, { 'e', 4 },
            { 'f', 5 }, { 'g', 6 }, { 'h', 7 }, { 'i', 8 }, { 'j', 9 },
            { 'k', 10 }, { 'l', 11 }, { 'm', 12 }, { 'n', 13 }, { 'o', 14 },
            { 'p', 15 }, { 'q', 16 }, { 'r', 17 }, { 's', 18 }, { 't', 19 },
            { 'u', 20 }, { 'v', 21 }, { 'w', 22 }, { 'x', 23 }, { 'y', 24 },
            { 'z', 25 },
            { 'A', 26 }, { 'B', 27 }, { 'C', 28 }, { 'D', 29 }, { 'E', 30 },
            { 'F', 31 }, { 'G', 32 }, { 'H', 33 }, { 'I', 34 }, { 'J', 35 },
            { 'K', 36 }, { 'L', 37 }, { 'M', 38 }, { 'N', 39 }, { 'O', 40 },
            { 'P', 41 }, { 'Q', 42 }, { 'R', 43 }, { 'S', 44 }, { 'T', 45 },
            { 'U', 46 }, { 'V', 47 }, { 'W', 48 }, { 'X', 49 }, { 'Y', 50 },
            { 'Z', 51 },
            { '1', 52 }, { '2', 53 }, { '3', 54 }, { '4', 55 }, { '5', 56 },
            { '6', 57 }, { '7', 58 }, { '8', 59 }, { '9', 60 }, { '0', 61 },
            { 'ä', 62 }, { 'Ä', 63 }, { 'ö', 64 }, { 'Ö', 65 }, { 'ü', 66 }, { 'Ü', 67 }, { 'ß', 68 },
            { '.', 69 }, { ':', 70 }, { ',', 71 }, { ';', 72 }, { '\'', 73 },
            { '$', 74 }, // Weird H thingy?
            { '"', 75 }, { '?', 76 }, { '!', 77 }, { '/', 78 }, { '(', 79 }, { ')', 80 },
            { '#', 81 }, { '%', 82 }, { '*', 83 }, { '&', 84 }, { '+', 85 }, { '-', 86 },
            { '=', 87 }, { '>', 88 }, { '<', 89 },
            { '^', 90 }, // Little skull / face glyph?
            { '♂', 91 }, { '♀', 92 }, // Male & female
            { 'é', 93 }, { 'â', 94 }, { 'à', 95 }, { 'ç', 96 }, { 'ê', 97 }, { 'ë', 98 }, { 'è', 99 },
            { 'ï', 100 }, { 'î', 101 }, { 'ì', 102 }, { 'ô', 103 }, { 'ò', 104 },
            { 'û', 105 }, { 'ù', 106 }, { 'á', 107 }, { 'í', 108 }, { 'ó', 109 }, { 'ú', 110 },
        };

        readonly string _text;
        readonly Vector2 _position;
        MultiSprite _sprite;
        Vector2 _resolution = Vector2.One;

        static readonly Handler[] Handlers =
        {
            new Handler<Text, RenderEvent>((x,e) => x.Render(e)),
            // TODO: Work out how to ensure this gets set when text created in between resize events.
            new Handler<Text, WindowResizedEvent>((x,e) =>
            {
                x._resolution = new Vector2(e.Width, e.Height);
                x.Reformat();
            }),
        };

        readonly ITexture _font;


        void Render(RenderEvent renderEvent)
        {
            renderEvent.Add(_sprite);
        }

        void Reformat()
        {
            _font.GetSubImageDetails(0, out var size, out _, out var texSize, out _);
            var instances = new SpriteInstanceData[_text.Length];
            for (int i = 0; i < _text.Length; i++)
            {
                char c = _text[i];
                if (FontMapping.TryGetValue(c, out var index))
                {
                    _font.GetSubImageDetails(index, out size, out var texOffset, out texSize, out var layer);
                    size = new Vector2(size.X, -size.Y);
                    instances[i].Flags =
                        SpriteFlags.UsePalette | 
                        SpriteFlags.NoTransform | 
                        SpriteFlags.Transparent;
                    instances[i].TexPosition = texOffset;
                    instances[i].TexSize = texSize;
                    instances[i].TexLayer = layer;
                    instances[i].Offset = new Vector3(_position + new Vector2(8 * size.X * i, 0) / _resolution, 0);
                    instances[i].Size = 8 * size / _resolution;
                }
            }

            _sprite = new MultiSprite(new SpriteKey(_font, (int)DrawLayer.Interface, false))
            {
                Instances = instances
            };
        }

        public Text(ITexture font, string text, Vector2 position) : base(Handlers)
        {
            // TODO: Left & right justification, kerning, line wrapping
            _text = text;
            _position = position;
            _font = font;
            Reformat();
        }
    }
}
