using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.State;
using UAlbion.Game.Text;

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

        static readonly HandlerSet Handlers = new HandlerSet(
            H<TextManager, TextEvent>((x,e) => x.OnTextEvent(e))
        );

        public TextManager() : base(Handlers) { }

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
                    var size = font.GetSubImageDetails(index).Size;
                    offset += (int)size.X;
                    if (block.Style == TextStyle.Fat || block.Style == TextStyle.FatAndHigh)
                        offset++;
                }
                else offset += SpaceSize;
            }

            var fontSize = font.GetSubImageDetails(0).Size;
            return new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
        }

        public PositionedSpriteBatch BuildRenderable(TextBlock block, DrawLayer order, object caller)
        {
            var assets = Resolve<IAssetManager>();
            var sm = Resolve<ISpriteManager>();
            var window = Resolve<IWindowManager>();

            var font = assets.LoadFont(block.Color, block.Style == TextStyle.Big);
            var text = block.Text ?? "";
            var isFat = block.Style == TextStyle.Fat || block.Style == TextStyle.FatAndHigh;

            int offset = 0;
            var flags = SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest;
            var key = new SpriteKey(font, order, flags);
            int displayableCharacterCount = text.Count(x => FontMapping.ContainsKey(x));
            int instanceCount = displayableCharacterCount * (isFat ? 4 : 2);
            var lease = sm.Borrow(key, instanceCount, caller);
            var instances = lease.Access();

            int n = 0;
            foreach (var c in text)
            {
                if (!FontMapping.TryGetValue(c, out var index)) { offset += SpaceSize; continue; } // Spaces etc

                var subImage = font.GetSubImageDetails(index);

                // Adjust texture coordinates slightly to avoid bleeding
                // var texOffset = subImage.TexOffset.Y + 0.1f / font.Height;

                var normPosition = window.UiToNormRelative(new Vector2(offset, 0));
                var baseInstance = SpriteInstanceData.TopLeft(
                    new Vector3(normPosition, 0),
                    window.UiToNormRelative(subImage.Size),
                    subImage, 0);

                instances[n] = baseInstance;
                instances[n + 1] = baseInstance;
                if (isFat)
                {
                    instances[n + 2] = baseInstance;
                    instances[n + 3] = baseInstance;

                    instances[n].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(2, 1)), 0));
                    instances[n].Flags |= SpriteFlags.DropShadow;

                    instances[n + 1].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1, 1)), 0));
                    instances[n + 1].Flags |= SpriteFlags.DropShadow;

                    instances[n + 2].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1, 0)), 0));
                    offset += 1;
                }
                else
                {
                    instances[n].Flags |= SpriteFlags.DropShadow;
                    instances[n].OffsetBy(new Vector3(window.UiToNormRelative(new Vector2(1, 1)), 0));
                }

                offset += (int)subImage.Size.X;
                n += isFat ? 4 : 2;
            }

            var fontSize = font.GetSubImageDetails(0).Size;
            var size = new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
            return new PositionedSpriteBatch(lease, size);
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

        public ITextSource FormatTextEvent(TextEvent textEvent, FontColor color)
        {
            var id = textEvent.TextId;

            return new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var textFormatter = new TextFormatter(assets, settings.Gameplay.Language);
                return textFormatter
                    .Ink(color)
                    .Format(new StringId(textEvent.Source.Type, textEvent.Source.Id, id))
                    .Blocks;
            });
        }

        void OnTextEvent(TextEvent textEvent)
        {
            textEvent.Acknowledged = true;
            switch(textEvent.Location)
            {
                case TextEvent.TextLocation.TextInWindow:
                {
                    var dialog = AttachChild(new TextWindow(FormatTextEvent(textEvent, FontColor.Yellow)));
                    dialog.Closed += (sender, _) => textEvent.Complete();
                    break;
                }

                case TextEvent.TextLocation.TextInWindowWithPortrait:
                case TextEvent.TextLocation.TextInWindowWithPortrait2:
                {
                        
                    SmallPortraitId portraitId = textEvent.PortraitId ?? Resolve<IParty>().Leader.ToPortraitId();
                    var dialog = AttachChild(new TextWindow(FormatTextEvent(textEvent, FontColor.Yellow), portraitId));
                    dialog.Closed += (sender, _) => textEvent.Complete();
                    break;
                }

                case TextEvent.TextLocation.QuickInfo:
                    Raise(new DescriptionTextExEvent(FormatTextEvent(textEvent, FontColor.White)));
                    textEvent.Complete();
                    break;

                default:
                    Raise(new DescriptionTextExEvent(FormatTextEvent(textEvent, FontColor.White))); // TODO:
                    textEvent.Complete();
                    break;
            }
        }
    }
}
