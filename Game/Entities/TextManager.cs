using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class TextManager : Component, ITextManager
    {
        const int SpaceSize = 3;
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

        public TextManager() : base(null) { }

        public Vector2 Measure(TextBlock block)
        {
            int offset = 0;
            var assets = Resolve<IAssetManager>();
            var font = assets.LoadFont(block.Color, block.Style == TextStyle.Big);
            if(block.Text == null)
                return Vector2.Zero;

            foreach (var c in block.Text)
            {
                if (FontMapping.TryGetValue(c, out var index))
                {
                    font.GetSubImageDetails(index, out var size, out _, out _, out _);
                    offset += (int)size.X;
                    if (block.Style == TextStyle.Fat || block.Style == TextStyle.FatAndHigh)
                        offset++;
                }
                else offset += SpaceSize;
            }

            font.GetSubImageDetails(0, out var fontSize, out _, out _, out _);
            return new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
        }

        public IPositionedRenderable BuildRenderable(TextBlock block, out Vector2 size)
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();

            var font = assets.LoadFont(block.Color, block.Style == TextStyle.Big);
            var text = block.Text ?? "";
            var isFat = block.Style == TextStyle.Fat || block.Style == TextStyle.FatAndHigh;

            var instances = new SpriteInstanceData[text.Length * (isFat ? 4 : 2)];
            int offset = 0;
            var flags = SpriteFlags.UsePalette | SpriteFlags.NoTransform | SpriteFlags.NoDepthTest;

            for (int i = 0; i < text.Length; i++)
            {
                int n = i * (isFat ? 4 : 2);
                char c = text[i];
                if (FontMapping.TryGetValue(c, out var index))
                {
                    font.GetSubImageDetails(index, out var characterSize, out var texOffset, out var texSize, out var layer);

                    // Adjust texture coordinates slightly to avoid bleeding
                    texOffset.Y += 0.1f / font.Height;

                    var normPosition = window.UiToNormRelative(new Vector2(offset, 0));
                    var baseInstance = SpriteInstanceData.TopLeft(
                        new Vector3(normPosition, 0),
                        window.UiToNormRelative(new Vector2(characterSize.X, characterSize.Y)),
                        texOffset, texSize, layer, flags);

                    instances[n] = baseInstance;
                    instances[n+1] = baseInstance;
                    if(isFat)
                    {
                        instances[n + 2] = baseInstance;
                        instances[n + 3] = baseInstance;

                        instances[n].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(2, 1)), 0));
                        instances[n].Flags |= SpriteFlags.DropShadow;

                        instances[n+1].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1,1)), 0));
                        instances[n+1].Flags |= SpriteFlags.DropShadow;

                        instances[n + 2].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1, 0)), 0));
                        offset += 1;
                    }
                    else
                    {
                        instances[n].Flags |= SpriteFlags.DropShadow;
                        instances[n].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1,1)), 0));
                    }

                    offset += (int)characterSize.X;
                }
                else
                {
                    offset += SpaceSize;
                }
            }

            font.GetSubImageDetails(0, out var fontSize, out _, out _, out _);
            size = new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
            return new UiMultiSprite(new SpriteKey(font, (int)DrawLayer.Interface, flags)) { Instances = instances };
        }

        public IEnumerable<TextBlock> SplitBlocksToSingleWords(IEnumerable<TextBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Arrangement.HasFlag(TextArrangement.NoWrap))
                {
                    yield return block;
                    continue;
                }

                var parts = block.Text.Split(' ');
                bool first = true;
                foreach (var part in parts)
                {
                    if (!first)
                    {
                        yield return new TextBlock(" ")
                        {
                            Alignment = block.Alignment,
                            Color = block.Color,
                            Style = block.Style,
                            Arrangement = block.Arrangement & ~TextArrangement.ForceNewLine
                        };

                        if (part.Length > 0)
                        {
                            yield return new TextBlock(part)
                            {
                                Alignment = block.Alignment,
                                Color = block.Color,
                                Style = block.Style,
                                Arrangement = block.Arrangement & ~TextArrangement.ForceNewLine
                            };
                        }
                    }
                    else
                    {
                        yield return new TextBlock(part)
                        {
                            Alignment = block.Alignment,
                            Color = block.Color,
                            Style = block.Style,
                            Arrangement = block.Arrangement
                        };
                        first = false;
                    }
                }
            }
        }
    }
}