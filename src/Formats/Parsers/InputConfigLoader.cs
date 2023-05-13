using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers;

public class InputConfigLoader : IAssetLoader<InputConfig>
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context) 
        => Serdes((InputConfig)existing, info, s, context);

    public InputConfig Serdes(InputConfig existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsWriting())
            return InputConfig.Load(info.File.Filename, context.Disk, context.Json);

        if (existing == null) throw new ArgumentNullException(nameof(existing));
        existing.Save(info.File.Filename, context.Disk, context.Json);
        return existing;

    }
}