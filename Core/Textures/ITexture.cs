namespace UAlbion.Core.Textures
{
    public interface ITexture
    {
        string Name { get; }
        uint Width { get; }
        uint Height { get; }
        uint Depth { get; }
        uint MipLevels { get; }
        uint ArrayLayers { get; }
        bool IsDirty { get; }
        int SubImageCount { get; }
        SubImage GetSubImageDetails(int subImage);
        int SizeInBytes { get; }
        uint FormatSize { get; }
    }
}
