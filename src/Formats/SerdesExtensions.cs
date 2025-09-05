using SerdesNet;

namespace UAlbion.Formats;

public static class SerdesExtensions
{
    // Fine to pass in a length in bytes as Albion's code-page 850 encoding is strictly a single-byte encoding
    public static string AlbionString(this ISerdes s, SerdesName name, string value, int fieldLengthInBytes)
    {
        byte[] bytes = null;
        if (s.IsWriting())
            bytes = FormatUtil.BytesFrom850StringN(value ?? "", fieldLengthInBytes);

        bytes = s.Bytes(name, bytes, fieldLengthInBytes);

        if (s.IsReading())
            value = FormatUtil.BytesTo850String(bytes);

        if (s.IsCommenting())
            s.Comment(value, true);

        return value!;
    }
}