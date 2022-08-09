using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text;

public class TextManager : ServiceComponent<ITextManager>, ITextManager
{
    public Vector2 Measure(TextBlock block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        int offset = 0;
        var assets = Resolve<IAssetManager>();
        var font = assets.LoadFont(block.Style == TextStyle.Big ? Base.Font.Bold : Base.Font.Regular, block.InkId);
        if (block.Text == null)
            return Vector2.Zero;

        for (var index = 0; index < block.Text.Length; index++)
        {
            var c = block.Text[index];
            int nextIndex = index + 1;
            var nextChar = nextIndex < block.Text.Length ? block.Text[nextIndex] : ' ';
            offset += font.GetAdvance(c, nextChar);
            if (block.Style is TextStyle.Fat or TextStyle.FatAndHigh)
                offset++;
        }

        return new Vector2(offset + 1, font.Definition.LineHeight + 1); // +1 for the drop shadow
    }

    public PositionedSpriteBatch BuildRenderable(TextBlock block, DrawLayer order, Rectangle? scissorRegion, object caller)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        var sm = Resolve<ISpriteManager<SpriteInfo>>();
        var assets = Resolve<IAssetManager>();
        var window = Resolve<IWindowManager>();

        var font = assets.LoadFont(block.Style == TextStyle.Big ? Base.Font.Bold : Base.Font.Regular, block.InkId);
        var text = block.Text ?? "";
        var isFat = block.Style is TextStyle.Fat or TextStyle.FatAndHigh;

        int offset = 0;
        var flags = SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.ClampEdges;
        scissorRegion = scissorRegion == null ? null : Resolve<IWindowManager>().UiToPixel(scissorRegion.Value);
        var key = new SpriteKey(font.Texture, SpriteSampler.Point, order, flags, scissorRegion);

        int displayableCharacterCount = 0;
        foreach (var c in text)
            if (font.SupportsCharacter(c))
                displayableCharacterCount++;

        int instanceCount = displayableCharacterCount * (isFat ? 4 : 2);
        var lease = sm.Borrow(key, instanceCount, caller);

        bool lockWasTaken = false;
        var instances = lease.Lock(ref lockWasTaken);
        try
        {
            int n = 0;
            for (var index = 0; index < text.Length; index++)
            {
                var c = text[index];
                int nextIndex = index + 1;
                var nextChar = nextIndex < block.Text.Length ? block.Text[nextIndex] : ' ';

                if (!font.SupportsCharacter(c))
                {
                    offset += font.Definition.SpaceSize;
                    continue;
                }

                var subImage = font.GetRegion(c);

                // Adjust texture coordinates slightly to avoid bleeding
                // var texOffset = subImage.TexOffset.Y + 0.1f / font.Height;

                var normPosition = window.UiToNormRelative(offset, 0);
                var baseInstance = new SpriteInfo(SpriteFlags.TopLeft, new Vector3(normPosition, 0),
                    window.UiToNormRelative(subImage.Size), subImage);

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

                offset += font.GetAdvance(c, nextChar);
                n += isFat ? 4 : 2;
            }
        }
        finally { lease.Unlock(lockWasTaken); }

        var size = new Vector2(offset + 1, font.Definition.LineHeight + 1); // +1 for the drop shadow
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
                        InkId = block.InkId,
                        Style = block.Style,
                        ArrangementFlags = block.ArrangementFlags & ~TextArrangementFlags.ForceNewLine
                    };

                    if (part.Length > 0)
                    {
                        yield return new TextBlock(block.BlockId, part)
                        {
                            Alignment = block.Alignment,
                            InkId = block.InkId,
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
                        InkId = block.InkId,
                        Style = block.Style,
                        ArrangementFlags = block.ArrangementFlags
                    };
                    first = false;
                }
            }
        }
    }
}