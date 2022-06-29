using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class TextManager : ServiceComponent<ITextManager>, ITextManager
{
    const int SpaceSize = 3;
    readonly Dictionary<SpriteId, Dictionary<char, int>> _fontMappings = new();
    readonly object _syncRoot = new();

    public Vector2 Measure(TextBlock block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        int offset = 0;
        var assets = Resolve<IAssetManager>();
        var font = assets.LoadFont(block.Color, block.Style == TextStyle.Big);
        if (block.Text == null)
            return Vector2.Zero;

        var mapping = GetFontMapping(font.Id, assets);
        foreach (var c in block.Text)
        {
            if (mapping.TryGetValue(c, out var index))
            {
                var size = font.Regions[index].Size;
                offset += (int)size.X;
                if (block.Style is TextStyle.Fat or TextStyle.FatAndHigh)
                    offset++;
            }
            else offset += SpaceSize;
        }

        var fontSize = font.Regions[0].Size;
        return new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
    }

    public PositionedSpriteBatch BuildRenderable(TextBlock block, DrawLayer order, Rectangle? scissorRegion, object caller)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        var assets = Resolve<IAssetManager>();
        var sm = Resolve<ISpriteManager<SpriteInfo>>();
        var window = Resolve<IWindowManager>();

        var font = assets.LoadFont(block.Color, block.Style == TextStyle.Big);
        var mapping = GetFontMapping(font.Id, assets);
        var text = block.Text ?? "";
        var isFat = block.Style is TextStyle.Fat or TextStyle.FatAndHigh;

        int offset = 0;
        var flags = SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.ClampEdges;
        var key = new SpriteKey(font, SpriteSampler.Point, order, flags, scissorRegion);

        int displayableCharacterCount = 0;
        foreach (var c in text)
            if (mapping.ContainsKey(c))
                displayableCharacterCount++;

        int instanceCount = displayableCharacterCount * (isFat ? 4 : 2);
        var lease = sm.Borrow(key, instanceCount, caller);

        bool lockWasTaken = false;
        var instances = lease.Lock(ref lockWasTaken);
        try
        {
            int n = 0;
            foreach (var c in text)
            {
                if (!mapping.TryGetValue(c, out var index)) { offset += SpaceSize; continue; } // Spaces etc

                var subImage = font.Regions[index];

                // Adjust texture coordinates slightly to avoid bleeding
                // var texOffset = subImage.TexOffset.Y + 0.1f / font.Height;

                var normPosition = window.UiToNormRelative(offset, 0);
                var baseInstance = new SpriteInfo(SpriteFlags.TopLeft, new Vector3(normPosition, 0), window.UiToNormRelative(subImage.Size), subImage);

                instances[n] = baseInstance;
                instances[n + 1] = baseInstance;
                if (isFat)
                {
                    instances[n + 2] = baseInstance;
                    instances[n + 3] = baseInstance;

                    instances[n].OffsetBy(new Vector3(window.UiToNormRelative(2, 1), 0));
                    instances[n].Flags |= SpriteFlags.DropShadow;

                    instances[n + 1].OffsetBy(new Vector3(window.UiToNormRelative(1, 1), 0));
                    instances[n + 1].Flags |= SpriteFlags.DropShadow;

                    instances[n + 2].OffsetBy(new Vector3(window.UiToNormRelative(1, 0), 0));
                    offset += 1;
                }
                else
                {
                    instances[n].Flags |= SpriteFlags.DropShadow;
                    instances[n].OffsetBy(new Vector3(window.UiToNormRelative(1, 1), 0));
                }

                offset += (int)subImage.Size.X;
                n += isFat ? 4 : 2;
            }
        }
        finally { lease.Unlock(lockWasTaken); }

        var fontSize = font.Regions[0].Size;
        var size = new Vector2(offset + 1, fontSize.Y + 1); // +1 for the drop shadow
        return new PositionedSpriteBatch(lease, size);
    }

    public IEnumerable<TextBlock> SplitBlocksToSingleWords(IEnumerable<TextBlock> blocks)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));
        foreach (var block in blocks)
        {
            if (block.ArrangementFlags.HasFlag(TextArrangementFlags.NoWrap))
            {
                yield return block;
                continue;
            }

            var parts = block.Text.Trim().Split(' ');
            bool first = true;
            foreach (var part in parts)
            {
                if (!first)
                {
                    yield return new TextBlock(block.BlockId, " ")
                    {
                        Alignment = block.Alignment,
                        Color = block.Color,
                        Style = block.Style,
                        ArrangementFlags = block.ArrangementFlags & ~TextArrangementFlags.ForceNewLine
                    };

                    if (part.Length > 0)
                    {
                        yield return new TextBlock(block.BlockId, part)
                        {
                            Alignment = block.Alignment,
                            Color = block.Color,
                            Style = block.Style,
                            ArrangementFlags = block.ArrangementFlags & ~TextArrangementFlags.ForceNewLine
                        };
                    }
                }
                else
                {
                    yield return new TextBlock(block.BlockId, part)
                    {
                        Alignment = block.Alignment,
                        Color = block.Color,
                        Style = block.Style,
                        ArrangementFlags = block.ArrangementFlags
                    };
                    first = false;
                }
            }
        }

    }
    Dictionary<char, int> GetFontMapping(IAssetId fontId, IAssetManager assets)
    {
        var id = AssetId.FromUInt32(fontId.ToUInt32());
        lock (_syncRoot)
        {
            if (_fontMappings.TryGetValue(id, out var cachedMapping))
                return cachedMapping;

            var info = assets.GetAssetInfo(id);
            var mappingString = info.Get<string>(AssetProperty.Mapping, null);
            if (mappingString == null)
                throw new InvalidOperationException($"The asset configuration for font {id} did not contain a Mapping property");

            var mapping = new Dictionary<char, int>();
            for (int i = 0; i < mappingString.Length; i++)
                mapping[mappingString[i]] = i;

            _fontMappings[id] = mapping;
            return mapping;
        }
    }
}