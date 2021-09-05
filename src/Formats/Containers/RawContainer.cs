using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Simple file containing a single asset.
    /// </summary>
    public class RawContainer : IAssetContainer
    {
        public ISerializer Read(string file, AssetInfo info, IFileSystem disk, IJsonUtil jsonUtil)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            ApiUtil.Assert(info.Index == 0, "SubItem should always be 0 when accessing a non-container file");
            var stream = disk.OpenRead(file);
            var br = new BinaryReader(stream);
            return new AlbionReader(br);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk, IJsonUtil jsonUtil)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            if (assets.Count == 0)
            {
                if (disk.FileExists(path))
                    disk.DeleteFile(path);
                return;
            }

            if (assets.Count > 1) throw new ArgumentOutOfRangeException(nameof(assets), "A RawContainer can only hold a single asset");

            var (_, bytes) = assets.Single();
            disk.WriteAllBytes(path, bytes);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk, IJsonUtil jsonUtil)
            => new() { (0, 1) };
    }
}
