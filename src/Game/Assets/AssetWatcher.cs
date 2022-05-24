#if false
using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Assets;

public sealed class AssetWatcher : Component, IDisposable
{
    readonly List<FileSystemWatcher> _watchers = new();

    public void AddPath(string modPath, AssetConfig assetConfig)
    {
        var watcher = new FileSystemWatcher(modPath);
        watcher.Changed += (sender, e) =>
        {
            var relative = Path.GetRelativePath(modPath, e.FullPath);
            relative = relative.Replace("\\", "/", StringComparison.Ordinal);

            foreach (var file in assetConfig.Files)
            {
                if (!relative.StartsWith(file.Key, StringComparison.InvariantCultureIgnoreCase)) 
                    continue;

                foreach (var asset in file.Value.Map)
                    Raise(new RefreshAssetEvent(asset.Value.AssetId));
            }
        };

        watcher.EnableRaisingEvents = true;
        _watchers.Add(watcher);
    }

    public void Clear()
    {
        foreach (var watcher in _watchers)
            watcher.Dispose();
        _watchers.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
#endif