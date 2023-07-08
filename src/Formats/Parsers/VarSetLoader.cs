using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class VarSetLoader : IAssetLoader<VarSet>
{
    public VarSet Serdes(VarSet existing, ISerializer s, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!context.Disk.FileExists(context.Filename))
            throw new FileNotFoundException($"Could not find game config file at expected path {context.Filename}");

        if (s.IsWriting())
            Save(existing, context.Filename, context.Disk, context.Json);
        else
            existing = Load(context.ModName, context.Filename, context.Disk, context.Json);

        return existing;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((VarSet)existing, s, context);

    public static VarSet Load(string name, string path, IFileSystem disk, IJsonUtil json)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (json == null) throw new ArgumentNullException(nameof(json));

        var bytes = disk.ReadAllBytes(path);
        return VarSet.FromJsonBytes(name, bytes, json);
    }

    public static void Save(VarSet set, string path, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (set == null) throw new ArgumentNullException(nameof(set));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var json = set.ToJson(jsonUtil);
        disk.WriteAllText(path, json);
    }
}