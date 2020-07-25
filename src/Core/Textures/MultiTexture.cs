using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Textures
{
    public abstract class MultiTexture : ITexture
    {
        protected readonly IPaletteManager PaletteManager;

        protected class SubImageComponent
        {
            public ITexture Source { get; set; }
            public uint X { get; set; }
            public uint Y { get; set; }
            public uint? W { get; set; }
            public uint? H { get; set; }
            public byte Alpha { get; set; } = 0xff;
            public override string ToString() => $"({X}, {Y}) {Source}";
        }

        protected class LogicalSubImage
        {
            public LogicalSubImage(int id) { Id = id; }

            public int Id { get; }
            public uint W { get; set; }
            public uint H { get; set; }
            public int Frames { get; set; }
            public bool IsPaletteAnimated { get; set; }
            public bool IsAlphaTested { get; set; }
            public byte? TransparentColor { get; set; }
            public IList<SubImageComponent> Components { get; } = new List<SubImageComponent>();

            public override string ToString() => $"LSI{Id} {W}x{H}:{Frames}{(IsPaletteAnimated ? "P":" ")} {string.Join("; ",  Components.Select(x => x.ToString()))}";
        }

        protected struct LayerKey : IEquatable<LayerKey>
        {
            public LayerKey(int id, int frame)
            {
                Id = id;
                Frame = frame;
            }

            public int Id { get; }
            public int Frame { get; }

            public bool Equals(LayerKey other) => Id == other.Id && Frame == other.Frame;
            public override bool Equals(object obj) => obj is LayerKey other && Equals(other);
            public override int GetHashCode() { unchecked { return (Id * 397) ^ Frame; } }
            public override string ToString() => $"LK{Id}.{Frame}";
        }

        protected readonly IList<LogicalSubImage> LogicalSubImages = new List<LogicalSubImage>();
        protected readonly IDictionary<LayerKey, int> LayerLookup = new Dictionary<LayerKey, int>();
        protected readonly IList<Vector2> LayerSizes = new List<Vector2>();
        protected bool IsMetadataDirty = true;
        bool _isAnySubImagePaletteAnimated;
        int _lastPaletteVersion;
        int _lastPaletteId;
        bool _isDirty;

        public MultiTexture(string name, IPaletteManager paletteManager)
        {
            PaletteManager = paletteManager;
            Name = name;
            MipLevels = 1; //(uint)Math.Min(Math.Log(Width, 2.0), Math.Log(Height, 2.0));

            // Add empty texture for disabled walls/ceilings etc
            LogicalSubImages.Add(new LogicalSubImage(0) { W = 1, H = 1, Frames = 1, IsPaletteAnimated = false });
        }

        public abstract uint FormatSize { get; }
        public string Name { get; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get { if (IsMetadataDirty) RebuildLayers(); return (uint)LayerSizes.Count; } }
        public int SubImageCount => LayerSizes.Count;

        public bool IsDirty
        {
            get
            {
                var version = PaletteManager.Version;
                if ((_isAnySubImagePaletteAnimated && version != _lastPaletteVersion) || PaletteManager.Palette.Id != _lastPaletteId)
                {
                    _lastPaletteVersion = version;
                    _lastPaletteId = PaletteManager.Palette.Id;
                    return true;
                }

                return _isDirty;
            }
            protected set => _isDirty = value;
        }

        public int SizeInBytes => (int)(Width * Height * LayerSizes.Count * FormatSize);

        public bool IsAnimated(int logicalId)
        {
            if(IsMetadataDirty)
                RebuildLayers();

            if (logicalId >= LogicalSubImages.Count)
                return false;

            return LogicalSubImages[logicalId].Frames > 1;
        }

        public int GetSubImageAtTime(int logicalId, int tick)
        {
            if(IsMetadataDirty)
                RebuildLayers();

            if (logicalId >= LogicalSubImages.Count)
                return 0;

            var logicalImage = LogicalSubImages[logicalId];
            if (LayerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var result))
                return result;
            return 0;
        }

        public SubImage GetSubImageDetails(int subImageId)
        {
            if(IsMetadataDirty)
                RebuildLayers();

            var size = LayerSizes[subImageId];
            return new SubImage(
                Vector2.Zero,
                size,
                new Vector2(Width, Height),
                (uint)subImageId);
        }

        protected void RebuildLayers()
        {
            _isAnySubImagePaletteAnimated = false;
            IsMetadataDirty = false;
            LayerLookup.Clear();
            LayerSizes.Clear();

            var palette = PaletteManager.Palette.GetCompletePalette();
            var animatedRange =
                palette
                    .SelectMany(x => x.Select((y, i) => (y, i)))
                    .GroupBy(x => x.i)
                    .Where(x => x.Distinct().Count() > 1)
                    .Select(x => (byte)x.Key)
                    .ToHashSet();

            foreach (var lsi in LogicalSubImages)
            {
                lsi.W = 1;
                lsi.H = 1;

                foreach (var component in lsi.Components)
                {
                    if (component.Source == null)
                        continue;
                    var size = component.Source.GetSubImageDetails(0).Size;
                    if (component.W.HasValue) size.X = component.W.Value;
                    if (component.H.HasValue) size.Y = component.H.Value;

                    if (lsi.W < component.X + size.X)
                        lsi.W = component.X + (uint)size.X;
                    if (lsi.H < component.Y + size.Y)
                        lsi.H = component.Y + (uint)size.Y;

                    if (!lsi.IsPaletteAnimated && component.Source is EightBitTexture t)
                        lsi.IsPaletteAnimated = t.ContainsColors(animatedRange);

                    if (lsi.IsPaletteAnimated)
                        _isAnySubImagePaletteAnimated = true;
                }

                lsi.Frames = (int)ApiUtil.Lcm(lsi.Components.Select(x => (long)x.Source.SubImageCount).Append(1));
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
                throw new InvalidOperationException("Too many textures added to multi-texture");
        }

        public void AddTexture(int logicalId, ITexture texture, uint x, uint y, byte? transparentColor, bool isAlphaTested, uint? w = null, uint? h = null, byte alpha = 255)
        {
            if(logicalId == 0)
                throw new InvalidOperationException("Logical Subimage Index 0 is reserved for a blank / transparent state");

            if (texture == null) // Will just end up using layer 0
                return;

            while(LogicalSubImages.Count <= logicalId)
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
        static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
        */

        protected void Rebuild(LogicalSubImage lsi, int frameNumber, Span<uint> toBuffer, IList<uint[]> palette)
        {
            foreach (var component in lsi.Components)
            {
                if (component.Source == null)
                    continue;

                var eightBitTexture = (EightBitTexture)component.Source;
                int frame = frameNumber % eightBitTexture.SubImageCount;
                eightBitTexture.GetSubImageOffset(frame, out var sourceWidth, out var sourceHeight, out var sourceOffset, out var sourceStride);
                uint destWidth = component.W ?? (uint)sourceWidth;
                uint destHeight = component.H ?? (uint)sourceHeight;

                if (component.X + destWidth > Width || component.Y + destHeight > Height)
                {
                    CoreTrace.Log.Warning(
                        "MultiTexture",
                        $"Tried to write an oversize component to {Name}: {component.Source.Name}:{frame} is ({destWidth}x{destHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                    continue;
                }

                ReadOnlySpan<byte> fromSlice = ((ReadOnlySpan<byte>)eightBitTexture.TextureData).Slice(
                    sourceOffset,
                    sourceWidth + (sourceHeight - 1) * sourceStride);

                Span<uint> toSlice = toBuffer.Slice(
                    (int)(component.Y * Width + component.X),
                    (int)(destWidth + (destHeight - 1) * Width));

                var from = new ReadOnlyByteImageBuffer((uint)sourceWidth, (uint)sourceHeight, (uint)sourceStride, fromSlice);
                var to = new UIntImageBuffer(destWidth, destHeight, (int)Width, toSlice);
                CoreUtil.Blit8To32(from, to, palette[PaletteManager.Frame], component.Alpha, lsi.TransparentColor);
            }
        }

        public abstract void SavePng(int logicalId, int tick, string path);
    }
}
