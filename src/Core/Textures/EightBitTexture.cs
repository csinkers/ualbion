/*
namespace UAlbion.Core.Textures;
public abstract class EightBitTexture : ITexture, IImage<byte>
{
    protected readonly byte[] _pixelData;
    public IAssetId Id { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int Depth => 1;
    public int MipLevels { get; }
    public int ArrayLayers { get; }
    public int SubImageCount => _subImages.Count;
    public bool IsDirty { get; protected set; }
    public IReadOnlyList<SubImage> SubImages { get; }
    public int SizeInBytes => _pixelData.Length;
    public PixelFormat Format => PixelFormat.EightBit;
    public int FormatSize => Format.Size();
    public ReadOnlySpan<byte> PixelData => _pixelData;
    public override string ToString() => $"8Bit {Name} ({Width}x{Height}, {_subImages.Count} subimages)";

    readonly List<SubImage> _subImages = new List<SubImage>();

    public EightBitTexture(
        IAssetId id,
        string name,
        int width,
        int height,
        int mipLevels,
        int arrayLayers,
        ReadOnlySpan<byte> textureData,
        IEnumerable<SubImage> subImages)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        MipLevels = mipLevels;
        ArrayLayers = arrayLayers;
        _pixelData = textureData.ToArray();
        if (subImages != null)
            foreach (var subImage in subImages)
                _subImages.Add(subImage);
        IsDirty = true;
        SubImages = _subImages.AsReadOnly();
    }

    public bool ContainsColors(IEnumerable<byte> colors) => _pixelData.Distinct().Intersect(colors).Any();
    public ISet<byte> DistinctColors(int? subImageId)
    {
        if (subImageId == null)
        {
            var buffer = new ReadOnlyImageBuffer<byte>(Width, Height, Width, PixelData);
            return BlitUtil.DistinctColors(buffer);
        }
        else
        {
            GetSubImageOffset(subImageId.Value, out var width, out var height, out var offset, out var stride);
            ReadOnlySpan<byte> slice = _pixelData.AsSpan(
                offset,
                width + (height - 1) * stride);
            var buffer = new ReadOnlyImageBuffer<byte>(width, height, stride, slice);
            return BlitUtil.DistinctColors(buffer);
        }
    }

    public void Invalidate() => IsDirty = true;

    public void GetSubImageOffset(int id, out int width, out int height, out int offset, out int stride)
    {
        var subImage = (SubImage)GetSubImage(id);
        if (subImage == null)
        {
            width = 0; height = 0; offset = 0; stride = 0;
            return;
        }

        width = subImage.Width;
        height = subImage.Height;
        offset = subImage.PixelOffset;
        stride = Width;
    }

    public ISubImage GetSubImage(int id)
    {
        if (_subImages.Count == 0)
            return null;

        if (id < 0)
            id = _subImages.Count + id;

        if (id >= _subImages.Count)
            id %= _subImages.Count;

        return _subImages[id];
    }

    public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
    {
        uint ret = largestLevelDimension;
        for (int i = 0; i < mipLevel; i++)
            ret /= 2;

        return Math.Max(1, ret);
    }

    public ReadOnlyImageBuffer<byte> GetSubImageBuffer(int i)
    {
        var frame = GetSubImage(i);
        ReadOnlySpan<byte> fromSlice = _pixelData.AsSpan(frame.PixelOffset, frame.PixelLength);
        return new ReadOnlyImageBuffer<byte>(frame.Width, frame.Height, Width, fromSlice);
    }
}
*/