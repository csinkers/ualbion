using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UAlbion.Config;

public class PathResolver : IPathResolver
{
    const string ModsDirectory = "mods";
    static readonly Regex Pattern = new(@"(\$\([A-Z]+\))");
    public string BasePath { get; }
    IDictionary<string, string> Paths { get; } = new Dictionary<string, string>();

    public PathResolver(string baseDir, string appName)
    {
        if (string.IsNullOrEmpty(appName))
            throw new ArgumentNullException(nameof(appName));

        BasePath = baseDir;
        Paths["CONFIG"] = Path.Combine(GetConfigBaseDir(), appName);
        Paths["CACHE"] = Path.Combine(GetCacheBaseDir(), appName);
        Paths["MODS"] = Path.Combine(baseDir, ModsDirectory);

        foreach (var kvp in Paths.ToList())
            if (!Path.IsPathRooted(kvp.Value))
                Paths[kvp.Key] = Path.Combine(baseDir, kvp.Value);
    }

    public void RegisterPath(string name, string path)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        Paths[name] = path;
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

        if (relative.Contains("..", StringComparison.Ordinal))
            throw new ArgumentOutOfRangeException($"Paths containing '..' are not allowed: {relative}");

        if (Path.IsPathRooted(relative) && !relative.StartsWith(BasePath, StringComparison.Ordinal))
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (configHome == null)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                configHome = Path.Combine(home, ".config");
            }

            if (configHome == null)
                throw new FileNotFoundException("Could not find a suitable location for config storage based on environment variables XDG_CONFIG_HOME / HOME");

            if (!Path.IsPathRooted(configHome))
                throw new InvalidOperationException($"Found config path {configHome}, but it is not absolute");

            return configHome;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(home, "Library", "Application Support");
        }

        throw new NotSupportedException();
    }

    static string GetCacheBaseDir()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

        throw new NotSupportedException(
            $"An implementation of {nameof(PathResolver)}.{nameof(GetCacheBaseDir)} has not been " +
            $"added for the operating system \"{RuntimeInformation.OSDescription}\" yet");
    }
}
