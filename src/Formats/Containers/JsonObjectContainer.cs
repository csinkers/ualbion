using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Container encoding a list of assets to a JSON object/dictionary
/// </summary>
public class JsonObjectContainer : IAssetContainer
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The serializer will handle it")]
    public ISerializer Read(string path, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!context.Disk.FileExists(path))
            return null;

        var dict = Load(path, context);
        if (!dict.TryGetValue(context.AssetId, out var token))
            return null;

        var ms = new MemoryStream(Encoding.UTF8.GetBytes(context.Json.Serialize(token)));
        var br = new BinaryReader(ms);
        return new GenericBinaryReader(
            br,
            ms.Length,
            Encoding.UTF8.GetString,
            ApiUtil.Assert,
            () => { br.Dispose(); ms.Dispose(); });
    }

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
    {
        if (assets == null) throw new ArgumentNullException(nameof(assets));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        var dict = new Dictionary<string, object>();
        foreach (var (info, bytes) in assets)
        {
            var jObject = context.Json.Deserialize<object>(bytes);
            dict[info.AssetId.ToString()] = jObject;
        }

        var fullText = context.Json.Serialize(dict);
        context.Disk.WriteAllText(path, fullText);
    }

    static IDictionary<AssetId, object> Load(string path, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var text = context.Disk.ReadAllBytes(path);
        var dict = context.Json.Deserialize<IDictionary<string, object>>(text);
        if (dict == null)
            throw new FileLoadException($"Could not deserialize \"{path}\"");

        return dict.ToDictionary(x => context.Mapping.Parse(x.Key, null), x => x.Value);
    }
}