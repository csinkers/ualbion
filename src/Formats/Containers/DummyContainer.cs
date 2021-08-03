using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    public class DummyContainer : IAssetContainer
    {
        public ISerializer Read(string path, AssetInfo info, IFileSystem disk)
        {
            var ms = new MemoryStream(new byte[] { 0 });
            var br = new BinaryReader(ms);
            return new AlbionReader(br, 1, () => { br.Dispose(); ms.Dispose(); });
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk) { }
        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk) => new() { (0, 1) };
    }
}