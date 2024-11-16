using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Container encoding a list of strings to a JSON object/dictionary
/// </summary>
public class JsonStringContainer : IAssetContainer
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The serializer will handle it")]
    public ISerdes Read(string path, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Disk.FileExists(path))
            return null;

        var dict = Load(path, context);
        if (!dict.TryGetValue(context.AssetId, out var value))
            return null;

        var ms = new MemoryStream(Encoding.UTF8.GetBytes(value));
        var br = new BinaryReader(ms);
        return new ReaderSerdes(
            br,
            ms.Length,
            Encoding.UTF8.GetString,
            ApiUtil.Assert,
            () => { br.Dispose(); ms.Dispose(); });
    }

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(context);

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        var dict = new Dictionary<string, string>();
        foreach(var (info, bytes) in assets)
            dict[info.AssetId.ToString()] = Encoding.UTF8.GetString(bytes);

        var fullText = context.Json.Serialize(dict);
        context.Disk.WriteAllText(path, fullText);
    }

    static Dictionary<AssetId, string> Load(string path, AssetLoadContext context)
    {
        var text = context.Disk.ReadAllBytes(path);
        var dict = context.Json.Deserialize<IDictionary<string, string>>(text);
        if (dict == null)
            throw new FileLoadException($"Could not deserialize \"{path}\"");

        var dictionary = new Dictionary<AssetId, string>();
        foreach (var pair in dict)
            dictionary.Add(AssetId.Parse(pair.Key), pair.Value);
        return dictionary;
    }
}