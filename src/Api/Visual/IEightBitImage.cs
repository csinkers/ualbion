namespace UAlbion.Api.Visual
{
    public interface IEightBitImage : IImage
    {
        byte[] PixelData { get; }
        ReadOnlyByteImageBuffer GetSubImageBuffer(int i);
    }
}