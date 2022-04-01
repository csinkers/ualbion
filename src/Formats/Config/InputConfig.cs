using System;
using System.Collections.Generic;
using UAlbion.Api;

namespace UAlbion.Formats.Config;

public class InputConfig
{
    public IDictionary<InputMode, IDictionary<string, string>> Bindings { get; private set; }
    public static InputConfig Load(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var inputConfig = new InputConfig();
        if (disk.FileExists(configPath))
        {
            var configText = disk.ReadAllBytes(configPath);
            inputConfig.Bindings = jsonUtil.Deserialize<IDictionary<InputMode, IDictionary<string, string>>>(configText);
        }

        return inputConfig;
    }

    public void Save(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var json = jsonUtil.Serialize(this);
        disk.WriteAllText(configPath, json);
    }
}