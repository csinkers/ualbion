using System;
using System.IO;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.BinaryOffsets)]
    public class BinaryOffsetContainerLoader : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            using var stream = File.OpenRead(file);
            using var br = new BinaryReader(stream);
            stream.Position = info.Offset ?? 0;
            var bytes = br.ReadBytes((info.Width ?? 0) * (info.Height ?? 0));
            var ms = new MemoryStream(bytes);
            return new AlbionReader(new BinaryReader(ms));
        }
    }
}
