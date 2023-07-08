using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using UAlbion.Api;
using UAlbion.Config.Properties;

namespace UAlbion.Config;

public class AssetConfigLoader
{
    readonly IJsonUtil _jsonUtil;
    readonly IFileSystem _disk;
    readonly IPathResolver _pathResolver;
    readonly TypeConfig _typeConfig;

    public AssetConfigLoader(
        IFileSystem disk,
        IJsonUtil jsonUtil,
        IPathResolver pathResolver,
        TypeConfig typeConfig)
    {
        _jsonUtil = jsonUtil;
        _disk = disk;
        _pathResolver = pathResolver;
        _typeConfig = typeConfig;
    }

    public AssetConfig Load(string configPath, string modName, AssetConfig parent)
    {
        if (!_disk.FileExists(configPath))
            throw new FileNotFoundException($"Could not open asset config from {configPath}");

        var configText = _disk.ReadAllBytes(configPath);
        try
        {
            var config = Parse(configText, modName, parent);
            if (config == null)
                throw new FileLoadException($"Could not load asset config from \"{configPath}\"");

            return config;
        }
        catch (Exception ex)
        {
            var message = $@"Error loading asset config ""{configPath}"":
  {ex.Message}";
            throw new AssetConfigLoadException(message, ex);
        }
    }

    public AssetConfig Parse(byte[] configText, string modName, AssetConfig parent)
    {
        var raw = _jsonUtil.Deserialize<Dictionary<string, LoadAssetRangeInfo>>(configText);
        if (raw == null)
            return null;

        var ranges = BuildRanges(raw, parent);
        var assetConfig = new AssetConfig(modName, ranges);
        return assetConfig;
    }

    RangeLookup BuildRanges(Dictionary<string, LoadAssetRangeInfo> raw, AssetConfig parent)
    {
        var rangeInfos = new List<AssetRangeInfo>();
        foreach (var kvp in raw)
            rangeInfos.Add(BuildRange(kvp.Key, kvp.Value));

        return new RangeLookup(parent?.Ranges, rangeInfos);
    }

    AssetRangeInfo BuildRange(string key, LoadAssetRangeInfo load)
    {
        AssetRange range = default;
        try
        {
            range = _typeConfig.ParseIdRange(key);
            var rangeInfo = new AssetRangeInfo(range);

            if (load.Properties != null)
                rangeInfo.Node.SetProperties(load.Properties, _typeConfig, "range", range);

            foreach (var fileKvp in load.Files)
                rangeInfo.Files.Add(BuildFile(rangeInfo, fileKvp.Key, fileKvp.Value));

            return rangeInfo;
        }
        catch (Exception ex)
        {
            var message = $@"Error loading info for range {key} ({range}):
  {ex.Message}";
            throw new AssetConfigLoadException(message, ex);
        }
    }

    AssetFileInfo BuildFile(AssetRangeInfo rangeInfo, string key, LoadAssetFileInfo load)
    {
        try
        {
            int index = key.IndexOf('#', StringComparison.Ordinal);
            var filename = index == -1 ? key : key[..index];
            var hash = index != -1
                ? key[(index + 1)..]
                : null;

            var fileInfo = new AssetFileInfo(rangeInfo);

            fileInfo.Node.SetProperty(AssetProps.Filename, filename);
            if (hash != null)
                fileInfo.Node.SetProperty(AssetProps.Sha256Hash, hash);

            if (load.Properties != null) // Do this before loading any asset mapping or the loader/container might not be set
                fileInfo.Node.SetProperties(load.Properties, _typeConfig, "file", key);

            if (load.Map != null)
            {
                foreach (var kvp in load.Map)
                {
                    var info = BuildAssetInfo(_typeConfig, kvp.Key, fileInfo, kvp.Value);
                    fileInfo.Map.Add(info.Id, info);
                }
            }

            if (load.MapFile != null)
            {
                var fullPath = _pathResolver.ResolvePath(load.MapFile);
                if (!_disk.FileExists(fullPath))
                    throw new FileNotFoundException($"Could not find mapping file \"{load.MapFile}\" ({fullPath}) specified in mod \"{_typeConfig.ModName}\"");

                var mappingJson = _disk.ReadAllBytes(fullPath);
                var mapping = _jsonUtil.Deserialize<Dictionary<string, LoadAssetInfo>>(mappingJson);
                foreach (var kvp in mapping)
                {
                    var info = BuildAssetInfo(_typeConfig, kvp.Key, fileInfo, kvp.Value);
                    fileInfo.Map.Add(info.Id, info);
                }
            }

            return fileInfo;
        }
        catch (Exception ex)
        {
            var message = $@"Error loading info for file {key}:
  {ex.Message}";
            throw new AssetConfigLoadException(message, ex);
        }
    }

    static AssetInfo BuildAssetInfo(TypeConfig typeConfig, string key, AssetFileInfo parent, LoadAssetInfo load)
    {
        AssetId id = AssetId.None;
        try
        {
            id = typeConfig.ResolveId(key);
            var result = new AssetInfo(id, parent);

            if (load.Properties != null)
                result.Node.SetProperties(load.Properties, typeConfig, "asset", id);

            return result;
        }
        catch (Exception ex)
        {
            var message = $@"Error loading info for asset {key} ({id}):
  {ex.Message}";
            throw new AssetConfigLoadException(message, ex);
        }
    }
}