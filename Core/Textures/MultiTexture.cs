using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace UAlbion.Core.Textures
{
    public class MultiTexture : ITexture
    {
        class SubImageComponent
        {
            public ITexture Source { get; set; }
            public uint X { get; set; }
            public uint Y { get; set; }
            public override string ToString() => $"({X}, {Y}) {Source}";
        }

        class LogicalSubImage
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
        bool _isMetadataDirty = true;
        bool _isAnySubImagePaletteAnimated = false;
        int _paletteFrame;
        public int PaletteFrame { set { _paletteFrame = value; IsDirty |= _isAnySubImagePaletteAnimated; } }

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
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get { if (_isMetadataDirty) RebuildLayers(); return (uint)_layerSizes.Count; } }
        public int SubImageCount => _layerSizes.Count;
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

        unsafe void Blit8To32(
            int width, int height, 
            byte* fromBuffer, uint* toBuffer, 
            int fromStride, int toStride,
            uint[] palette, byte? transparentColor)
        {
            byte* from = fromBuffer;
            uint* to = toBuffer;
            if (transparentColor.HasValue)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        if (*from != transparentColor.Value)
                            *to = palette[*from];
                        to++;
                        from++;
                    }

                    from += (fromStride - width);
                    to += (toStride - width);
                }
            }
            else
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        *to = palette[*from];
                        to++;
                        from++;
                    }

                    from += (fromStride - width);
                    to += (toStride - width);
                }
            }
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        static extern void ZeroMemory(IntPtr dest, IntPtr size);

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            bool rebuildAll = _isMetadataDirty;
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
                        //if (!rebuildAll && !lsi.IsPaletteAnimated) // TODO: Requires caching a single Texture and then modifying it
                        //    continue;

                        for (int i = 0; i < lsi.Frames; i++)
                        {
                            if(lsi.IsAlphaTested)
                                ZeroMemory((IntPtr)toBuffer, (IntPtr)(Width * Height * sizeof(uint)));
                            else
                            {
                                for(int j = 0; j < Width*Height;j++)
                                    toBuffer[j] = 0xff000000;
                            }

                            uint destinationLayer = (uint)_layerLookup[new LayerKey(lsi.Id, i)];

                            foreach (var component in lsi.Components)
                            {
                                if (component.Source == null)
                                    continue;

                                var eightBitTexture = (EightBitTexture)component.Source;
                                int frame = i % eightBitTexture.SubImageCount;
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
                                    Blit8To32(
                                        sourceWidth,
                                        sourceHeight,
                                        fromBuffer + sourceOffset,
                                        toBuffer + (int)(component.Y * Width + component.X),
                                        sourceStride,
                                        (int)Width,
                                        _palette[_paletteFrame],
                                        lsi.TransparentColor);
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
            _isAnySubImagePaletteAnimated = false;
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

                    if (lsi.IsPaletteAnimated)
                        _isAnySubImagePaletteAnimated = true;
                }

                lsi.Frames = (int)Api.Util.LCM(lsi.Components.Select(x => (long)x.Source.SubImageCount).Append(1));
                for (int i = 0; i < lsi.Frames; i++)
                {
                    _layerLookup[new LayerKey(lsi.Id, i)] = _layerSizes.Count;
                    _layerSizes.Add(new Vector2(lsi.W, lsi.H));
                }

                if (Width < lsi.W)
                    Width = lsi.W;
                if (Height < lsi.H)
                    Height = lsi.H;
            }

            if (_layerSizes.Count > 255)
                throw new InvalidOperationException("Too many textures added to multi-texture");
        }

        public void AddTexture(int logicalId, ITexture texture, uint x, uint y, byte? transparentColor, bool isAlphaTested)
        {
            if(logicalId == 0)
                throw new InvalidOperationException("Logical Subimage Index 0 is reserved for a blank / transparent state");

            if (texture == null) // Will just end up using layer 0
                return;

            while(_logicalSubImages.Count <= logicalId)
                _logicalSubImages.Add(new LogicalSubImage(logicalId));

            var lsi = _logicalSubImages[logicalId];
            lsi.IsAlphaTested = isAlphaTested;
            lsi.TransparentColor = transparentColor;
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

        public void SavePng(int logicalId, int tick, string path)
        {
            if(_isMetadataDirty)
                RebuildLayers();

            var logicalImage = _logicalSubImages[logicalId];
            if (_layerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var subImageId))
            {
                var size = _layerSizes[subImageId];
                int width = (int)size.X;
                int height = (int)size.Y;
                Rgba32[] pixels = new Rgba32[width * height];

                foreach (var component in logicalImage.Components)
                {
                    if (component.Source == null)
                        continue;

                    var eightBitTexture = (EightBitTexture)component.Source;
                    int frame = tick % eightBitTexture.SubImageCount;
                    eightBitTexture.GetSubImageOffset(frame, out var sourceWidth, out var sourceHeight,
                        out var sourceOffset, out var sourceStride);

                    if (component.X + sourceWidth > Width || component.Y + sourceHeight > Height)
                    {
                        CoreTrace.Log.Warning(
                            "MultiTexture",
                            $"Tried to write an oversize component to {Name}: {component.Source.Name}:{frame} is ({sourceWidth}x{sourceHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                        continue;
                    }

                    unsafe
                    {
                        fixed (Rgba32* toBuffer = &pixels[0])
                        fixed (byte* fromBuffer = &eightBitTexture.TextureData[0])
                        {
                            Blit8To32(
                                    sourceWidth,
                                    sourceHeight,
                                    fromBuffer + sourceOffset,
                                    (uint*)toBuffer + (int)(component.Y * Width + component.X),
                                    sourceStride,
                                    (int)Width,
                                    _palette[_paletteFrame],
                                    logicalImage.TransparentColor);
                        }
                    }
                }

                Image<Rgba32> image = new Image<Rgba32>(width, height);
                image.Frames.AddFrame(pixels);
                image.Frames.RemoveFrame(0);
                using(var stream = File.OpenWrite(path))
                    image.SaveAsBmp(stream);
            }
        }
    }
}