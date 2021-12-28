using System.IO;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats;

public class AlbionWriter : GenericBinaryWriter
{
    public AlbionWriter(BinaryWriter bw)
        : base(bw, FormatUtil.BytesFrom850String, ApiUtil.Assert)
    {
    }
}