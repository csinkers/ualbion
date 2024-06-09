namespace UAlbion.Formats.Exporters.Tiled;

public static class TileEncodings
{
    static readonly ITileEncoding[] Encodings =
    [
        UncompressedCsvTileEncoding.Instance,
        ZlibBase64TileEncoding.Instance
    ];

    public static ITileEncoding TryGetEncoding(string encoding, string compression)
    {
        foreach (var candidate in Encodings)
        {
            if (candidate.Encoding == encoding && candidate.Compression == compression)
                return candidate;
        }

        return null;
    }
}