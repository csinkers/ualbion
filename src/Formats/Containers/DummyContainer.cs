using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    public class DummyContainer : IAssetContainer
    {
        public ISerializer Read(string path, AssetInfo info)
        {
            var ms = new MemoryStream(new byte[] { 0 });
            var br = new BinaryReader(ms);
            return new AlbionReader(br, 1, () => { br.Dispose(); ms.Dispose(); });
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets) { }
        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info) => new List<(int, int)> { (0, 1) };
    }
}