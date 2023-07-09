using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers;

public class InputConfigLoader : IAssetLoader<InputConfig>
{
    public object Serdes(object existing, ISerializer s, AssetLoadContext context) 
        => Serdes((InputConfig)existing, s, context);

    public InputConfig Serdes(InputConfig existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsWriting())
            return InputConfig.Load(context.Filename, context.Disk, context.Json);

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        existing.Save(context.Filename, context.Disk, context.Json);
        return existing;
    }
}