using System;
using System.Diagnostics.CodeAnalysis;
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

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public static class Asset
{
    static readonly XldContainer XldLoader = new();
    public delegate T SerdesFunc<T>(T x, ISerdes s, AssetLoadContext context) where T : class;
    public static void Compare(
        string resultDir,
        string testName,
        ReadOnlyMemory<byte> originalBytes,
        ReadOnlyMemory<byte> roundTripBytes,
        (string, string)[] notes) // (extension, text)
    {
        if (string.IsNullOrEmpty(resultDir))
            throw new ArgumentNullException(nameof(resultDir));

        ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Asset size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
        ApiUtil.Assert(originalBytes.Span.SequenceEqual(roundTripBytes.Span));

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

    public static (T, string) Load<T>(ReadOnlyMemory<byte> bytes, SerdesFunc<T> serdes, AssetLoadContext context) where T : class
    {
        using var ar = AlbionSerdes.CreateReader(bytes);

        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var afs = new AnnotationProxySerdes(ar, annotationWriter);

        T result;
        Exception exception = null;
        try
        {
            result = serdes(null, afs, context);
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

    public static (ReadOnlyMemory<byte>, string) Save<T>(T asset, SerdesFunc<T> serdes, AssetLoadContext context) where T : class
    {
        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var writer = AlbionSerdes.CreateWriter();
        using var aw = new AnnotationProxySerdes(writer, annotationWriter);

        Exception exception = null;
        try { serdes(asset, aw, context); }
        catch (Exception ex) { exception = ex; }

        ;
        annotationWriter.Flush();
        var annotation = ReadToEnd(annotationStream);

        if (exception != null)
            throw new AssetSerializationException(exception, annotation);

        return (writer.GetMemory(), annotation);
    }

    public static string Load(byte[] bytes, Action<ISerdes> serdes)
    {
        using var ar = AlbionSerdes.CreateReader(bytes);

        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var afs = new AnnotationProxySerdes(ar, annotationWriter);

        Exception exception = null;
        try
        {
            serdes(afs);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        annotationWriter.Flush();
        var annotation = ReadToEnd(annotationStream);

        if (exception != null)
            throw new AssetSerializationException(exception, annotation);

        if (afs.BytesRemaining > 0)
            throw new InvalidOperationException($"{afs.BytesRemaining} bytes left over after reading");

        return annotation;
    }

    public static (ReadOnlyMemory<byte>, string) Save(Action<ISerdes> serdes)
    {
        using var annotationStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationStream);
        using var writer = AlbionSerdes.CreateWriter();
        using var aw = new AnnotationProxySerdes(writer, annotationWriter);

        Exception exception = null;
        try { serdes(aw); }
        catch (Exception ex) { exception = ex; }

        annotationWriter.Flush();
        var annotation = ReadToEnd(annotationStream);

        if (exception != null)
            throw new AssetSerializationException(exception, annotation);

        return (writer.GetMemory(), annotation);
    }

    public static string SaveJson(object asset, IJsonUtil jsonUtil)
    {
        ArgumentNullException.ThrowIfNull(jsonUtil);
        using var stream = new MemoryStream();
        stream.Write(Encoding.UTF8.GetBytes(jsonUtil.Serialize(asset)));
        return ReadToEnd(stream);
    }

    public static T LoadJson<T>(string json, IJsonUtil jsonUtil)
    {
        ArgumentNullException.ThrowIfNull(jsonUtil);
        return jsonUtil.Deserialize<T>(Encoding.UTF8.GetBytes(json));
    }

    public static byte[] BytesFromXld(IPathResolver conf, string path, AssetLoadContext context)
    {
        using var s = XldLoader.Read(conf.ResolvePath(path), context);
        return s.Bytes(null, null, (int)s.BytesRemaining);
    }
}