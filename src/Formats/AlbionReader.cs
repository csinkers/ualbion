using System.IO;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats
{
    public class AlbionReader : GenericBinaryReader
    {
        public AlbionReader(BinaryReader br, long maxLength = 0)
            : base(
                br,
                maxLength == 0 ? br.BaseStream.Length : maxLength,
                FormatUtil.BytesTo850String,
                ApiUtil.Assert)
        {
        }
    }
}
