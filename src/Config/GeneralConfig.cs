using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UAlbion.Api;

namespace UAlbion.Config;

public class GeneralConfig : IGeneralConfig
{
    const string ConfigSubdir = "ualbion";
    static readonly Regex Pattern = new(@"(\$\([A-Z]+\))");
    [JsonIgnore] public string BasePath { get; private set; }
    [JsonInclude] public IDictionary<string, string> Paths { get; private set; } = new Dictionary<string, string>();

    public static GeneralConfig Load(string configPath, string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var config = disk.FileExists(configPath)
            ? jsonUtil.Deserialize<GeneralConfig>(disk.ReadAllBytes(configPath))
            : new GeneralConfig();

        config.BasePath = baseDir;
        config.Paths["CONFIG"] = Path.Combine(GetConfigBaseDir(), ConfigSubdir);
        config.Paths["CACHE"] = Path.Combine(GetCacheBaseDir(), ConfigSubdir);

        foreach (var kvp in config.Paths.ToList())
            if (!Path.IsPathRooted(kvp.Value))
                config.Paths[kvp.Key] = Path.Combine(baseDir, kvp.Value);

        return config;
    }

    public string ResolvePathAbsolute(string relative)
    {
        var path = ResolvePath(relative);
        if (string.IsNullOrEmpty(path))
            return null;

        return !Path.IsPathRooted(path) 
            ? Path.Combine(BasePath, path) 
            : path;
    }

    public string GetPath(string pathName) => Paths.TryGetValue(pathName, out var result) ? result : null;

    public string ResolvePath(string relative)
    {
        if (string.IsNullOrEmpty(relative))
            throw new ArgumentNullException(nameof(relative));

        if (relative.Contains("..", StringComparison.InvariantCulture))
            throw new ArgumentOutOfRangeException($"Paths containing .. are not allowed: {relative}");

        if (Path.IsPathRooted(relative) && !relative.StartsWith(BasePath, StringComparison.InvariantCulture))
            throw new ArgumentOutOfRangeException($"Rooted paths outside outside the base relative UAlbion path ({BasePath}) are not allowed: {relative}");

        var resolved = Pattern.Replace(relative, x =>
        {
            var name = x.Groups[0].Value[2..].TrimEnd(')').ToUpperInvariant();

            if (Paths.TryGetValue(name, out var value))
                return value;

            throw new InvalidOperationException($"Could not find path substitution for {name} in path {relative}");
        });

        return resolved;
    }

    static string GetConfigBaseDir()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (configHome == null)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (home != null)
                    configHome = Path.Combine(home, ".config");
            }

            if (configHome == null)
                throw new FileNotFoundException("Could not find a suitable location for config storage based on environment variables XDG_CONFIG_HOME / HOME");

            if (!Path.IsPathRooted(configHome))
                throw new InvalidOperationException($"Found config path {configHome}, but it is not absolute");

            return configHome;
        }

        throw new NotSupportedException();
    }

    static string GetCacheBaseDir()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (cacheHome == null)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (home != null)
                    cacheHome = Path.Combine(home, ".config");
            }

            if (cacheHome == null)
                throw new FileNotFoundException("Could not find a suitable location for cache storage based on environment variables XDG_CACHE_HOME / HOME");

            if (!Path.IsPathRooted(cacheHome))
                throw new InvalidOperationException($"Found cache path {cacheHome}, but it is not absolute");

            return cacheHome;
        }

        throw new NotSupportedException();
    }
}