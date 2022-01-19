using System;
using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Containers;
using Xunit;

namespace UAlbion.TestCommon;

public static class Asset
{
    static readonly XldContainer XldLoader = new();
    public static void Compare(
        string resultDir,
        string testName,
        byte[] originalBytes,
        byte[] roundTripBytes,
        (string, string)[] notes) // (extension, text)
    {
        ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Asset size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
        ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

        var diffs = XDelta.Compare(originalBytes, roundTripBytes).ToArray();

        if (originalBytes.Length != roundTripBytes.Length || diffs.Length > 1)
        {
            if (!Directory.Exists(resultDir))
                Directory.CreateDirectory(resultDir);

            var path = Path.Combine(resultDir, testName);
            foreach (var (extension, text) in notes)
                if (!string.IsNullOrEmpty(text))
                    File.WriteAllText(path + extension, text);
        }

        Assert.Collection(diffs,
            d =>
            {
                Assert.True(d.IsCopy);
                Assert.Equal(0, d.Offset);
                Assert.Equal(originalBytes.Length, d.Length);
            });
    }

    static string ReadToEnd(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, null, true, -1, true);
        return reader.ReadToEnd();
    }

    public static (T, string) Load<T>(byte[] bytes, Func<T, ISerializer, T> serdes) where T : class
    {
        using var stream = new MemoryStream(bytes);
        using var br = new BinaryReader(stream);
        using var ar = new AlbionReader(br, stream.Length);

        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var afs = new AnnotationFacadeSerializer(ar, annotationWriter, FormatUtil.BytesFrom850String);

        T result;
        Exception exception = null;
        try
        {
            result = serdes(null, afs);
        }
        catch (Exception ex)
        {
            exception = ex;
            result = default;
        }

        annotationWriter.Flush();
        var annotation = ReadToEnd(annotationStream);

        if (exception != null)
            throw new AssetSerializationException(exception, annotation);

        if (afs.BytesRemaining > 0)
            throw new InvalidOperationException($"{afs.BytesRemaining} bytes left over after reading");

        return (result, annotation);
    }

    public static (byte[], string) Save<T>(T asset, Func<T, ISerializer, T> serdes) where T : class
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var aw = new AnnotationFacadeSerializer(new AlbionWriter(bw), annotationWriter, FormatUtil.BytesFrom850String);

        Exception exception = null;
        try { serdes(asset, aw); }
        catch (Exception ex) { exception = ex; }

        ms.Position = 0;
        var bytes = ms.ToArray();
        annotationWriter.Flush();
        var annotation = ReadToEnd(annotationStream);

        if (exception != null)
            throw new AssetSerializationException(exception, annotation);

        return (bytes, annotation);
    }

    public static string SaveJson(object asset, IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        using var stream = new MemoryStream();
        stream.Write(Encoding.UTF8.GetBytes(jsonUtil.Serialize(asset)));
        return ReadToEnd(stream);
    }

    public static T LoadJson<T>(string json, IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Deserialize<T>(Encoding.UTF8.GetBytes(json));
    }

    public static byte[] BytesFromXld(IGeneralConfig conf, string path, AssetInfo info, IFileSystem disk, IJsonUtil jsonUtil)
    {
        using var s = XldLoader.Read(conf.ResolvePath(path), info, disk, jsonUtil);
        return s.Bytes(null, null, (int)s.BytesRemaining);
    }
}