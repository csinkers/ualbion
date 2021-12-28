using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.Config;

public class InputConfig
{
    public IDictionary<InputMode, IDictionary<string, string>> Bindings { get; private set; }
    readonly string _basePath;

    public InputConfig(string basePath)
    {
        _basePath = basePath;
    }

    public static InputConfig Load(string basePath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var inputConfig = new InputConfig(basePath);
        var configPath = Path.Combine(basePath, "data", "input.json");
        if (disk.FileExists(configPath))
        {
            var configText = disk.ReadAllBytes(configPath);
            inputConfig.Bindings = jsonUtil.Deserialize<IDictionary<InputMode, IDictionary<string, string>>>(configText);
        }

        return inputConfig;
    }

    public void Save(IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var configPath = Path.Combine(_basePath, "data", "input.json");
        var json = jsonUtil.Serialize(this);
        disk.WriteAllText(configPath, json);
    }
}