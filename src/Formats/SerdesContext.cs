using System;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats;

public class SerdesContext
{
    public SerdesContext(string modName, IJsonUtil json, AssetMapping mapping, IFileSystem disk)
    {
        ModName = modName;
        Json = json ?? throw new ArgumentNullException(nameof(json));
        Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        Disk = disk ?? throw new ArgumentNullException(nameof(disk));
    }

    public string ModName { get; }
    public IJsonUtil Json { get; }
    public IFileSystem Disk { get; }
    public AssetMapping Mapping { get; }
}