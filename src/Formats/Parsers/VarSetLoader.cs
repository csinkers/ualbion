using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class VarSetLoader : IAssetLoader<VarSet>
{
    public VarSet Serdes(VarSet existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (!context.Disk.FileExists(info.File.Filename))
            throw new FileNotFoundException($"Could not find game config file at expected path {info.File.Filename}");

        return Load(info.File.Filename, context.Disk, context.Json);
    }

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((VarSet)existing, info, s, context);

    public static VarSet Load(string path, IFileSystem disk, IJsonUtil json)
    {
        var bytes = disk.ReadAllBytes(path);
        var dictionary = json.Deserialize<Dictionary<string, object>>(bytes);
        return new VarSet(dictionary);
    }
}