namespace UAlbion.Formats.Exporters.Tiled;

public interface ITileEncoding
{
    string Compression { get; }
    string Encoding { get; }
    string Encode(int[] data, int width);
    int[] Decode(string data);
}