namespace UAlbion.Api.Visual;

public interface IFont
{
    ReadOnlyImageBuffer<byte> GetRegionBuffer(char c);
    int GetAdvance(char c, char nextChar);
}