using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapperUtil
{
    public static string PropString(Map map, string key, bool required = false)
    {
        if (map.Properties == null || map.Properties.Count == 0)
        {
            if (required)
                throw new FormatException($"Map property \"{key}\" was not found");
            return null;
        }

        var prop = map.Properties.FirstOrDefault(x => key.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        return prop?.Value;
    }

    public static int? PropInt(Map map, string key) => int.TryParse(PropString(map, key), out var i) ? i : null;
    public static AssetId PropId(Map map, string key, bool required = false)
    {
        var id = AssetId.Parse(PropString(map, key));
        if (required && id.IsNone)
            throw new FormatException($"Map property \"{key}\" was invalid or not found");
        return id;
    }

    public static IEnumerable<int> ParseCsv(string csv)
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