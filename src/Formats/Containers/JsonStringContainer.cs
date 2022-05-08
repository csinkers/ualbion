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
/// Container encoding a list of strings to a JSON object/dictionary
/// </summary>
public class JsonStringContainer : IAssetContainer
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The serializer will handle it")]
    public ISerializer Read(string path, AssetInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!context.Disk.FileExists(path))
            return null;

        var dict = Load(path, context);
        if (!dict.TryGetValue(info.AssetId, out var value))
            return null;

        var ms = new MemoryStream(Encoding.UTF8.GetBytes(value));
        var br = new BinaryReader(ms);
        return new GenericBinaryReader(
            br,
            ms.Length,
            Encoding.UTF8.GetString,
            ApiUtil.Assert,
            () => { br.Dispose(); ms.Dispose(); });
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context)
    {
        if (assets == null) throw new ArgumentNullException(nameof(assets));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        var dict = new Dictionary<string, string>();
        foreach(var (info, bytes) in assets)
            dict[info.AssetId.ToString()] = Encoding.UTF8.GetString(bytes);

        var fullText = context.Json.Serialize(dict);
        context.Disk.WriteAllText(path, fullText);
    }

    public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!context.Disk.FileExists(path))
            return null;

        var dict = Load(path, context);
        return FormatUtil.SortedIntsToRanges(dict.Keys.Select(x => x.Id).OrderBy(x => x));
    }

    static IDictionary<AssetId, string> Load(string path, SerdesContext context)
    {
        var text = context.Disk.ReadAllBytes(path);
        var dict = context.Json.Deserialize<IDictionary<string, string>>(text);
        if (dict == null)
            throw new FileLoadException($"Could not deserialize \"{path}\"");

        return dict.ToDictionary(x => AssetId.Parse(x.Key), x => x.Value);
    }
}