using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UAlbion.Formats.Exporters.Tiled;

public class UncompressedCsvTileEncoding : ITileEncoding
{
    UncompressedCsvTileEncoding() { }
    public static UncompressedCsvTileEncoding Instance { get; } = new();

    public string Compression => null;
    public string Encoding => "csv";
    public string Encode(int[] data, int width)
    {
        var sb = new StringBuilder();
        int lineLength = 0;
        bool first = true;
        foreach (var tile in data)
        {
            if (!first)
                sb.Append(',');

            first = false;
            sb.Append(tile);
            lineLength++;

            if (lineLength == width)
            {
                sb.AppendLine();
                lineLength = 0;
            }
        }

        return sb.ToString();
    }

    public int[] Decode(string data) => ParseCsv(data).ToArray();
    static IEnumerable<int> ParseCsv(string csv)
    {
        if (string.IsNullOrEmpty(csv))
            yield break;

        int n = 0;
        foreach (var c in csv)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    n *= 10;
                    n += c - '0';
                    break;
                case ',':
                    yield return n;
                    n = 0;
                    break;
            }
        }
        yield return n;
    }
}