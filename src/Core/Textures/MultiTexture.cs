using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures
{
    public abstract class MultiTexture : ITexture
    {
        protected class SubImageComponent
        {
            public ITexture Source { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int? W { get; set; }
            public int? H { get; set; }
            public byte Alpha { get; set; } = 0xff;
            public override string ToString() => $"({X}, {Y}) {Source}";
        }

        protected class LogicalSubImage
        {
            public LogicalSubImage(int id) { Id = id; }

            public int Id { get; }
            public int W { get; set; }
            public int H { get; set; }
            public int Frames { get; set; }
            public bool IsAlphaTested { get; set; }
            public byte? TransparentColor { get; set; }
            public IList<SubImageComponent> Components { get; } = new List<SubImageComponent>();

            public override string ToString() => $"LSI{Id} {W}x{H}:{Frames} {string.Join("; ",  Components.Select(x => x.ToString()))}";
        }

        protected struct LayerKey : IEquatable<LayerKey>
        {
            readonly int _id;
            readonly int _frame;
            public LayerKey(int id, int frame) { _id = id; _frame = frame; }
            public bool Equals(LayerKey other) => _id == other._id && _frame == other._frame;
            public override bool Equals(object obj) => obj is LayerKey other && Equals(other);
            public static bool operator ==(LayerKey left, LayerKey right) => left.Equals(right);
            public static bool operator !=(LayerKey left, LayerKey right) => !(left == right);
            public override int GetHashCode() { unchecked { return (_id * 397) ^ _frame; } }
            public override string ToString() => $"LK{_id}.{_frame}";
        }

        protected IPalette Palette { get; }
        protected IList<LogicalSubImage> LogicalSubImages { get; }= new List<LogicalSubImage>();
        protected IDictionary<LayerKey, int> LayerLookup { get; }= new Dictionary<LayerKey, int>();
        protected IList<Vector2> LayerSizes { get; }= new List<Vector2>();
        protected bool IsMetadataDirty { get; private set; } = true;

        protected MultiTexture(IAssetId id, string name, IPalette palette)
        {
            Id = id;
            Name = name;
            Palette = palette ?? throw new ArgumentNullException(nameof(palette));
            MipLevels = 1; //(int)Math.Min(Math.Log(Width, 2.0), Math.Log(Height, 2.0));

            // Add empty texture for disabled walls/ceilings etc
            LogicalSubImages.Add(new LogicalSubImage(0) { W = 1, H = 1, Frames = 1 });
        }

        public PixelFormat Format => PixelFormat.Rgba32;
        public abstract int FormatSize { get; }
        public IAssetId Id { get; }
        public string Name { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth => 1;
        public int MipLevels { get; }
        public int ArrayLayers { get { if (IsMetadataDirty) RebuildLayers(); return LayerSizes.Count; } }
        public int SubImageCount => LayerSizes.Count;
        public bool IsDirty { get; protected set; }
        public int SizeInBytes => Width * Height * LayerSizes.Count * FormatSize;
        public void Invalidate() => IsDirty = true; 

        public int GetFrameCountForLogicalId(int logicalId)
        {
            if (IsMetadataDirty)
                RebuildLayers();

            return logicalId >= LogicalSubImages.Count ? 1 : LogicalSubImages[logicalId].Frames;
        }

        public int GetSubImageAtTime(int logicalId, int tick)
        {
            if (IsMetadataDirty)
                RebuildLayers();

            if (logicalId >= LogicalSubImages.Count)
                return 0;

            var logicalImage = LogicalSubImages[logicalId];
            return LayerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var result) ? result : 0;
        }

        public ISubImage GetSubImage(int subImage)
        {
            if (IsMetadataDirty)
                RebuildLayers();

            var size = LayerSizes[subImage];
            return new SubImage(
                Vector2.Zero,
                size,
                new Vector2(Width, Height),
                subImage);
        }

        protected void RebuildLayers()
        {
            IsMetadataDirty = false;
            LayerLookup.Clear();
            LayerSizes.Clear();

            foreach (var lsi in LogicalSubImages)
            {
                lsi.W = 1;
                lsi.H = 1;
                long frames = 1;

                foreach (var component in lsi.Components)
                {
                    if (component.Source == null)
                        continue;

                    var size = ((SubImage)component.Source.GetSubImage(0)).Size;
                    if (component.W.HasValue) size.X = component.W.Value;
                    if (component.H.HasValue) size.Y = component.H.Value;

                    if (lsi.W < component.X + size.X)
                        lsi.W = component.X + (int)size.X;
                    if (lsi.H < component.Y + size.Y)
                        lsi.H = component.Y + (int)size.Y;

                    frames = ApiUtil.Lcm(frames, component.Source.SubImageCount);
                    if (component.Source is IEightBitImage eightBit)
                    {
                        var colours = eightBit.PixelData.ToHashSet();
                        long paletteFrames = BlitUtil.CalculatePalettePeriod(colours, Palette);
                        frames = ApiUtil.Lcm(frames, paletteFrames);
                    }
                }

                lsi.Frames = (int)frames;
                for (int i = 0; i < lsi.Frames; i++)
                {
                    LayerLookup[new LayerKey(lsi.Id, i)] = LayerSizes.Count;
                    LayerSizes.Add(new Vector2(lsi.W, lsi.H));
                }

                if (Width < lsi.W)
                    Width = lsi.W;
                if (Height < lsi.H)
                    Height = lsi.H;
            }

            if (LayerSizes.Count > 255)
                ApiUtil.Assert($"Too many textures added to multi-texture: {LayerSizes.Count}");
        }

        public void AddTexture(
            int logicalId,
            ITexture texture,
            int x,
            int y,
            byte? transparentColor,
            bool isAlphaTested,
            int? w = null,
            int? h = null,
            byte alpha = 255)
        {
            if (logicalId == 0)
                throw new InvalidOperationException("Logical Subimage Index 0 is reserved for a blank / transparent state");

            if (texture == null) // Will just end up using layer 0
                return;

            while (LogicalSubImages.Count <= logicalId)
                LogicalSubImages.Add(new LogicalSubImage(logicalId));

            var lsi = LogicalSubImages[logicalId];
            lsi.IsAlphaTested = isAlphaTested;
            lsi.TransparentColor = transparentColor;
            lsi.Components.Add(new SubImageComponent
            {
                Source = texture,
                X = x,
                Y = y,
                W = w,
                H = h,
                Alpha = alpha
            });

            IsDirty = true;
            IsMetadataDirty = true;
        }

        /* TODO: Add mip-mapping
        static int GetDimension(int largestLevelDimension, int mipLevel)
        {
            int ret = largestLevelDimension;
            for (int i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
        */

        protected void Rebuild(LogicalSubImage lsi, int frameNumber, Span<uint> toBuffer, IList<uint[]> palette)
        {
            if (lsi == null) throw new ArgumentNullException(nameof(lsi));
            if (palette == null) throw new ArgumentNullException(nameof(palette));
            foreach (var component in lsi.Components)
            {
                if (component.Source == null)
                    continue;

                var eightBitTexture = (EightBitTexture)component.Source;
                int frame = frameNumber % eightBitTexture.SubImageCount;
                int palFrame = frameNumber % palette.Count;
                eightBitTexture.GetSubImageOffset(frame, out var sourceWidth, out var sourceHeight, out var sourceOffset, out var sourceStride);
                int destWidth = component.W ?? sourceWidth;
                int destHeight = component.H ?? sourceHeight;

                if (component.X + destWidth > Width || component.Y + destHeight > Height)
                {
                    CoreTrace.Log.Warning(
                        "MultiTexture",
                        $"Tried to write an oversize component to {Name}: {component.Source.Name}:{frame} is ({destWidth}x{destHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                    continue;
                }

                ReadOnlySpan<byte> fromSlice = eightBitTexture.PixelData.AsSpan(
                    sourceOffset,
                    sourceWidth + (sourceHeight - 1) * sourceStride);

                Span<uint> toSlice = toBuffer.Slice(
                    component.Y * Width + component.X,
                    destWidth + (destHeight - 1) * Width);

                var from = new ReadOnlyByteImageBuffer(sourceWidth, sourceHeight, sourceStride, fromSlice);
                var to = new UIntImageBuffer(destWidth, destHeight, Width, toSlice);
                BlitUtil.Blit8To32(from, to, palette[palFrame], component.Alpha, lsi.TransparentColor);
            }
        }

        public abstract void SavePng(int logicalId, int tick, string path, IFileSystem disk);
    }
}
