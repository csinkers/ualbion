using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vulkan;

namespace UAlbion.Core.Textures
{
    public class MultiTexture : ITexture
    {
        class SubImageComponent
        {
            public ITexture Source { get; set; }
            public uint X { get; set; }
            public uint Y { get; set; }
        }

        class LogicalSubImage
        {
            public LogicalSubImage(int id) { Id = id; }

            public int Id { get; }
            public uint W { get; set; }
            public uint H { get; set; }
            public int Frames { get; set; }
            public bool IsPaletteAnimated { get; set; }
            public IList<SubImageComponent> Components { get; } = new List<SubImageComponent>();
        }

        struct LayerKey : IEquatable<LayerKey>
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

        readonly IList<LogicalSubImage> _logicalSubImages = new List<LogicalSubImage>();
        readonly IDictionary<LayerKey, int> _layerLookup = new Dictionary<LayerKey, int>();
        readonly IList<Vector2> _layerSizes = new List<Vector2>();
        readonly IList<uint[]> _palette;
        readonly HashSet<byte> _animatedRange;
        uint _width;
        uint _height;
        bool _isMetadataDirty = true;

        public MultiTexture(string name, IList<uint[]> palette)
        {
            _palette = palette;
            _animatedRange =
                _palette
                    .SelectMany(x => x.Select((y, i) => (y, i)))
                    .GroupBy(x => x.i)
                    .Where(x => x.Distinct().Count() > 1)
                    .Select(x => (byte)x.Key)
                    .ToHashSet();
            Name = name;
            MipLevels = 1; //(uint)Math.Min(Math.Log(Width, 2.0), Math.Log(Height, 2.0));

            // Add empty texture for disabled walls/ceilings etc
            _logicalSubImages.Add(new LogicalSubImage(0) { W = 1, H = 1, Frames = 1, IsPaletteAnimated = false });
        }

        public string Name { get; }
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width { get { if (_isMetadataDirty) RebuildLayers(); return _width; } private set => _width = value; }
        public uint Height { get { if (_isMetadataDirty) RebuildLayers(); return _height; } private set => _height = value; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get { if (_isMetadataDirty) RebuildLayers(); return (uint)_layerSizes.Count; } }

        public bool IsDirty { get; private set; }

        public int GetSubImageAtTime(int logicalId, int tick)
        {
            if(_isMetadataDirty)
                RebuildLayers();

            if (logicalId >= _logicalSubImages.Count)
                return 0;

            var logicalImage = _logicalSubImages[logicalId];
            if (_layerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var result))
                return result;
            return 0;
        }

        public void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            if(_isMetadataDirty)
                RebuildLayers();

            size = _layerSizes[subImage];
            texOffset = Vector2.Zero;
            texSize = size / new Vector2(Width, Height);
            layer = (uint)subImage;
        }

        unsafe void Blit8To24(int width, int height, byte* fromBuffer, uint* toBuffer, int fromStride, int toStride, uint[] palette)
        {
            byte* from = fromBuffer;
            uint* to = toBuffer;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if(*from != 0)
                        *to = palette[*from];
                    to++; from++;
                }

                from += (fromStride - width);
                to += (toStride - width);
            }
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        static extern void ZeroMemory(IntPtr dest, IntPtr size);

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            if(_isMetadataDirty)
                RebuildLayers();

            using (Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type)))
            {
                staging.Name = "T_" + Name + "_Staging";

                unsafe
                {
                    uint* toBuffer = stackalloc uint[(int)(Width * Height)];
                    foreach (var lsi in _logicalSubImages)
                    {
                        for (int i = 0; i < lsi.Frames; i++)
                        {
                            ZeroMemory((IntPtr)toBuffer, (IntPtr)(Width * Height * sizeof(uint)));
                            uint destinationLayer = (uint)_layerLookup[new LayerKey(lsi.Id, i)];

                            foreach (var component in lsi.Components)
                            {
                                if (component.Source == null)
                                    continue;

                                var eightBitTexture = (EightBitTexture)component.Source;
                                int frame = i % (int)eightBitTexture.ArrayLayers;
                                int paletteFrame = lsi.IsPaletteAnimated ? i % _palette.Count : 0;
                                eightBitTexture.GetSubImageOffset(frame, out var sourceWidth, out var sourceHeight,
                                    out var sourceOffset, out var sourceStride);

                                if (component.X + sourceWidth > Width || component.Y + sourceHeight > Height)
                                {
                                    CoreTrace.Log.Warning(
                                        "MultiTexture",
                                        $"Tried to write an oversize component to {Name}: {component.Source.Name}:{frame} is ({sourceWidth}x{sourceHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                                    continue;
                                }

                                fixed (byte* fromBuffer = &eightBitTexture.TextureData[0])
                                {
                                    Blit8To24(
                                        sourceWidth,
                                        sourceHeight,
                                        fromBuffer + sourceOffset,
                                        toBuffer + (int)(component.Y * Width + component.X),
                                        sourceStride,
                                        (int)Width,
                                        _palette[paletteFrame]);
                                }
                            }

                            gd.UpdateTexture(
                                staging, (IntPtr)toBuffer, Width * Height * sizeof(uint),
                                0, 0, 0, Width, Height, 1,
                                0, destinationLayer);
                        }
                    }
                }

                /* TODO: Mipmap
                for (uint level = 1; level < MipLevels; level++)
                {
                } //*/

                var texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
                texture.Name = "T_" + Name;

                using (CommandList cl = rf.CreateCommandList())
                {
                    cl.Begin();
                    cl.CopyTexture(staging, texture);
                    cl.End();
                    gd.SubmitCommands(cl);
                }

                IsDirty = false;
                return texture;
            }
        }

        void RebuildLayers()
        {
            _isMetadataDirty = false;
            _layerLookup.Clear();
            _layerSizes.Clear();

            foreach (var lsi in _logicalSubImages)
            {
                lsi.W = 1;
                lsi.H = 1;

                foreach (var component in lsi.Components)
                {
                    if (component.Source == null)
                        continue;
                    component.Source.GetSubImageDetails(0, out var size, out _, out _, out _);
                    if (lsi.W < component.X + size.X)
                        lsi.W = component.X + (uint)size.X;
                    if (lsi.H < component.Y + size.Y)
                        lsi.H = component.Y + (uint)size.Y;

                    if (!lsi.IsPaletteAnimated && component.Source is EightBitTexture t)
                        lsi.IsPaletteAnimated = t.ContainsColors(_animatedRange);
                }

                lsi.Frames = (int)Api.Util.LCM(
                    lsi.Components.Select(x => (long)x.Source.ArrayLayers)
                        .Append(lsi.IsPaletteAnimated ? _palette.Count : 1));

                for(int i = 0; i < lsi.Frames; i++)
                {
                    _layerLookup[new LayerKey(lsi.Id, i)] = _layerSizes.Count;
                    _layerSizes.Add(new Vector2(lsi.W, lsi.H));

                    if (_layerSizes.Count > 255)
                        throw new InvalidOperationException("Too many textures added to multi-texture");
                }

                if (Width < lsi.W)
                    Width = lsi.W;
                if (Height < lsi.H)
                    Height = lsi.H;
            }
        }

        public void AddTexture(int logicalSubImage, ITexture texture, uint x, uint y)
        {
            if (texture == null) // Will just end up using layer 0
                return;

            while(_logicalSubImages.Count <= logicalSubImage)
                _logicalSubImages.Add(new LogicalSubImage(logicalSubImage));

            var lsi = _logicalSubImages[logicalSubImage];
            lsi.Components.Add(new SubImageComponent
            {
                Source = texture,
                X = x,
                Y = y
            });

            IsDirty = true;
            _isMetadataDirty = true;
        }

        uint GetFormatSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                case PixelFormat.R8_UNorm: return 1;
                case PixelFormat.R8_UInt: return 1;
                default: throw new NotImplementedException();
            }
        }

        static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }
}