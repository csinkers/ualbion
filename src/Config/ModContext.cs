using System;
using UAlbion.Api;

namespace UAlbion.Config;

public class ModContext
{
    public ModContext(string modName, IJsonUtil json, IFileSystem disk, AssetMapping mapping)
    {
        ModName = modName;
        Json = json ?? throw new ArgumentNullException(nameof(json));
        Disk = disk ?? throw new ArgumentNullException(nameof(disk));
        Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    public string ModName { get; }
    public IJsonUtil Json { get; }
    public IFileSystem Disk { get; }
    public AssetMapping Mapping { get; }
}