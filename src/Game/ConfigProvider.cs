using System;
using System.IO;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Config;

namespace UAlbion.Game;

public sealed class ConfigProvider : IConfigProvider, IDisposable
{
    readonly FileSystemWatcher _watcher;
    readonly string _baseDir;
    readonly IFileSystem _disk;
    readonly IJsonUtil _jsonUtil;
    public CoreConfig Core { get; private set; }
    public GameConfig Game { get; private set; }
    public InputConfig Input { get; private set; }

    string DataPath => Path.Combine(_baseDir, "data");
    string CorePath => Path.Combine(DataPath, "core.json");
    string GamePath => Path.Combine(DataPath, "game.json");
    string InputPath => Path.Combine(DataPath, "input.json");

    public event EventHandler<EventArgs> GameChanged;
    public event EventHandler<EventArgs> CoreChanged;
    public event EventHandler<EventArgs> InputChanged;

    public ConfigProvider(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
    {
        _baseDir = baseDir;
        _disk = disk ?? throw new ArgumentNullException(nameof(disk));
        _jsonUtil = jsonUtil ?? throw new ArgumentNullException(nameof(jsonUtil));
        _watcher = new FileSystemWatcher(DataPath);
        _watcher.Changed += (_, e) => Reload(e.FullPath);
        _watcher.EnableRaisingEvents = true;

        Reload(CorePath);
        Reload(GamePath);
        Reload(InputPath);
    }

    void Reload(string fullPath)
    {
        if (fullPath == GamePath)
        {
            Game = GameConfig.Load(GamePath, _disk, _jsonUtil);
            GameChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (fullPath == CorePath)
        {
            Core = CoreConfig.Load(CorePath, _disk, _jsonUtil);
            CoreChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (fullPath == InputPath)
        {
            Input = InputConfig.Load(InputPath, _disk, _jsonUtil);
            InputChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose() => _watcher.Dispose();
}