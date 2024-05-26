using System;
using System.Linq;
using UAlbion.Config;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapperUtil
{
    public static string PropString(ITiledPropertySource source, string key, bool required = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Properties == null || source.Properties.Count == 0)
        {
            if (required)
                throw new FormatException($"Property \"{key}\" was not found");
            return null;
        }

        var prop = source.Properties.FirstOrDefault(x => key.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        return prop?.Value;
    }

    public static bool? PropBool(ITiledPropertySource source, string key) => bool.TryParse(PropString(source, key), out var i) ? i : null;
    public static int? PropInt(ITiledPropertySource source, string key) => int.TryParse(PropString(source, key), out var i) ? i : null;
    public static AssetId PropId(ITiledPropertySource source, string key, bool required = false)
    {
        var id = AssetId.Parse(PropString(source, key));
        if (required && id.IsNone)
            throw new FormatException($"Property \"{key}\" was invalid or not found");
        return id;
    }
}