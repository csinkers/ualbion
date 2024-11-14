using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures;

public class CompositedTexture : IReadOnlyTexture<uint>
{
    const int LayerLimit = 512;

    readonly List<LogicalSubImage> _logicalSubImages = new();
    readonly Dictionary<LayerKey, int> _layerLookup = new();
    readonly List<Vector2> _layerSizes = new();
    IPalette _palette;
    bool _isMetadataDirty = true;
    ArrayTexture<uint> _texture;
    int _width;
    int _height;

    public CompositedTexture(IAssetId id, string name, IPalette palette)
    {
        Id = id;
        Name = name;
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));

        // Add empty texture for disabled walls/ceilings etc
        _logicalSubImages.Add(new LogicalSubImage(0) { W = 1, H = 1, Frames = 1 });
    }

    ArrayTexture<uint> Texture { get { Rebuild(); return _texture; } }
    public IAssetId Id { get; }
    public string Name { get; }
    public int Width { get { Rebuild(); return _width; } private set => _width = value; } 
    public int Height { get { Rebuild(); return _height; } private set => _height = value; }
    public int ArrayLayers => Texture.ArrayLayers;
    public int SizeInBytes => Texture.SizeInBytes;
    public int Version => Texture.Version;

    public IReadOnlyList<Region> Regions => Texture.Regions;
    public ReadOnlySpan<uint> PixelData => Texture.PixelData;

    public IPalette Palette
    {
        get => _palette;
        set
        {
            if (_palette == value) return;
            _palette = value;
            _isMetadataDirty = true;
        }
    }

    public ReadOnlyImageBuffer<uint> GetRegionBuffer(Region region) => Texture.GetRegionBuffer(region);
    public ReadOnlyImageBuffer<uint> GetRegionBuffer(int i) => Texture.GetRegionBuffer(i);
    public ReadOnlyImageBuffer<uint> GetLayerBuffer(int i) => Texture.GetLayerBuffer(i);

    public int GetFrameCountForLogicalId(int logicalId)
    {
        Rebuild();
        return logicalId >= _logicalSubImages.Count ? 1 : _logicalSubImages[logicalId].Frames;
    }

    public int GetSubImageAtTime(int logicalId, int tick, bool backAndForth)
    {
        Rebuild();

        if (logicalId >= _logicalSubImages.Count)
            return 0;

        var logicalImage = _logicalSubImages[logicalId];
        int frame;
        if (backAndForth && logicalImage.Frames > 2)
        {
            int maxFrame = logicalImage.Frames - 1;
            frame = tick % (2 * maxFrame) - maxFrame;
            frame = Math.Abs(frame);
        }
        else frame = tick % logicalImage.Frames;

        return _layerLookup.GetValueOrDefault(new LayerKey(logicalId, frame), 0);
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
        byte alpha = 255,
        int[] regions = null) 
    {
        if (logicalId == 0)
            throw new InvalidOperationException("Logical Subimage Index 0 is reserved for a blank / transparent state");

        if (texture == null) // Will just end up using layer 0
            return;

        while (_logicalSubImages.Count <= logicalId)
            _logicalSubImages.Add(new LogicalSubImage(logicalId));

        var lsi = _logicalSubImages[logicalId];
        lsi.IsAlphaTested = isAlphaTested;
        lsi.TransparentColor = transparentColor;
        lsi.Components.Add(new SubImageComponent
        {
            Texture = texture,
            X = x,
            Y = y,
            W = w,
            H = h,
            Alpha = alpha,
            Regions = regions
        });

        _isMetadataDirty = true;
    }

    public void RebuildAll() // Just for unit tests
    {
        _isMetadataDirty = true;
        Rebuild();
    }

    void Rebuild()
    {
        using var _ = PerfTracker.FrameEvent("6.1.2.1 Rebuild MultiTextures");
        if (!_isMetadataDirty)
            return;

        RebuildLayout();

        if (_texture == null || _texture.Width != Width || _texture.Height != Height || _texture.ArrayLayers != _layerSizes.Count)
            _texture = new ArrayTexture<uint>(Id, Name, Width, Height, _layerSizes.Count);

        foreach (var lsi in _logicalSubImages)
        {
            for (int i = 0; i < lsi.Frames; i++)
            {
                if (_layerLookup.TryGetValue(new LayerKey(lsi.Id, i), out var destinationLayer))
                {
                    Span<uint> toBuffer = _texture.GetMutableLayerBuffer(destinationLayer).Buffer;
                    toBuffer.Fill(lsi.IsAlphaTested ? 0 : 0xff000000);
                    RebuildLayer(lsi, i, toBuffer, Palette.Texture);
                }
            }
        }

        for (int i = 0; i < _layerSizes.Count; i++)
            _texture.AddRegion(Vector2.Zero, _layerSizes[i], i);
    }

    void RebuildLayout()
    {
        _isMetadataDirty = false;
        _layerLookup.Clear();
        _layerSizes.Clear();

        foreach (var lsi in _logicalSubImages)
        {
            lsi.W = 1;
            lsi.H = 1;
            long frames = 1;

            foreach (var component in lsi.Components)
            {
                if (component.Texture == null)
                    continue;

                var size = component.Texture.Regions[0].Size;
                if (component.W.HasValue) size.X = component.W.Value;
                if (component.H.HasValue) size.Y = component.H.Value;

                if (lsi.W < component.X + size.X)
                    lsi.W = component.X + (int)size.X;
                if (lsi.H < component.Y + size.Y)
                    lsi.H = component.Y + (int)size.Y;

                int componentFrameCount = component.Regions?.Length ?? component.Texture.Regions.Count;
                frames = ApiUtil.Lcm(frames, componentFrameCount);
                if (component.Texture is IReadOnlyTexture<byte> eightBit)
                {
                    var colours = new HashSet<byte>();
                    foreach (var pixel in eightBit.PixelData)
                        colours.Add(pixel);

                    long paletteFrames = BlitUtil.CalculatePalettePeriod(colours, Palette);
                    frames = ApiUtil.Lcm(frames, paletteFrames);
                }
            }

            lsi.Frames = (int)frames;
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

        if (_layerSizes.Count > LayerLimit)
        {
            ApiUtil.Assert($"Too many textures added to multi-texture: {_layerSizes.Count}");
            _layerSizes.RemoveRange(LayerLimit, _layerSizes.Count-LayerLimit);
            foreach (var key in _layerLookup.Keys.ToArray())
                if (_layerLookup[key] >= LayerLimit)
                    _layerLookup.Remove(key);
        }

        // Make sure that there's always multiple layers to stop OpenGL complaining about
        // being given a regular texture when it was expecting an array texture.
        if (_layerSizes.Count < 2)
            _layerSizes.Add(new Vector2(1, 1));
    }

    void RebuildLayer(LogicalSubImage lsi, int frameNumber, Span<uint> toBuffer, IReadOnlyTexture<uint> palette)
    {
        ArgumentNullException.ThrowIfNull(lsi);
        ArgumentNullException.ThrowIfNull(palette);

        foreach (var component in lsi.Components)
        {
            if (component.Texture == null)
                continue;

            int frame = component.Regions == null
                ? frameNumber % component.Texture.Regions.Count
                : component.Regions[frameNumber % component.Regions.Length];

            if (component.Texture is IReadOnlyTexture<byte> eightBitTexture)
            {
                int palFrame = frameNumber % palette.Height;

                var from = eightBitTexture.GetRegionBuffer(frame);
                int destWidth = component.W ?? from.Width;
                int destHeight = component.H ?? from.Height;

                if (component.X + destWidth > Width || component.Y + destHeight > Height)
                {
                    CoreTrace.Log.Warning(
                        "MultiTexture",
                        $"Tried to write an oversize component to {Name}: {component.Texture.Name}:{frame} is ({destWidth}x{destHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                    continue;
                }

                Span<uint> toSlice = toBuffer.Slice(
                    component.Y * Width + component.X,
                    destWidth + (destHeight - 1) * Width);

                var to = new ImageBuffer<uint>(destWidth, destHeight, Width, toSlice);
                BlitUtil.BlitTiled8To32(from, to, palette.GetRegionBuffer(palFrame).Buffer, component.Alpha, lsi.TransparentColor);
            }

            if (component.Texture is IReadOnlyTexture<uint> trueColorTexture)
            {
                var from = trueColorTexture.GetRegionBuffer(frame);
                int destWidth = component.W ?? from.Width;
                int destHeight = component.H ?? from.Height;

                if (component.X + destWidth > Width || component.Y + destHeight > Height)
                {
                    CoreTrace.Log.Warning(
                        "MultiTexture",
                        $"Tried to write an oversize component to {Name}: {component.Texture.Name}:{frame} is ({destWidth}x{destHeight}) @ ({component.X}, {component.Y}) but multitexture is only ({Width}x{Height})");
                    continue;
                }

                Span<uint> toSlice = toBuffer.Slice(
                    component.Y * Width + component.X,
                    destWidth + (destHeight - 1) * Width);

                var to = new ImageBuffer<uint>(destWidth, destHeight, Width, toSlice);
                BlitUtil.BlitTiled32(from, to);
            }
        }
    }
}