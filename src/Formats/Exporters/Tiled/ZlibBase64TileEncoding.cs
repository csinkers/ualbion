using System;

namespace UAlbion.Formats.Exporters.Tiled;

public class ZlibBase64TileEncoding : ITileEncoding
{
    ZlibBase64TileEncoding() { }
    public static ZlibBase64TileEncoding Instance { get; } = new();

    public string Compression => "zlib";
    public string Encoding => "base64";
    public string Encode(int[] data, int width)
    {
        var compressed = ZipUtil.Deflate(data);
        return Convert.ToBase64String(compressed);
    }

    public int[] Decode(string data)
    {
        var decoded = Convert.FromBase64String(data);
        return ZipUtil.Inflate(decoded);
    }
}