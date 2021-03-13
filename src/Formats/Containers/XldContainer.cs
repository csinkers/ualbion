using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Simple container, header contains file sizes, then followed by uncompressed raw file data.
    /// </summary>
    public class XldContainer : IAssetContainer
    {
        const string MagicString = "XLD0I";
        static int HeaderSize(int itemCount) => MagicString.Length + 3 + 4 * itemCount;

        public ISerializer Read(string file, AssetInfo info, IFileSystem disk)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            using var s = new AlbionReader(new BinaryReader(disk.OpenRead(file)));
            var bytes = LoadAsset(info.Index, s);
            var ms = new MemoryStream(bytes);
            return new AlbionReader(new BinaryReader(ms));
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            int count = assets.Max(x => x.Item1.Index) + 1;
            var ordered = new (AssetInfo, byte[])[count];
            var lengths = new int[count];
            foreach (var (info, bytes) in assets)
            {
                ordered[info.Index] = (info, bytes);
                lengths[info.Index] = bytes.Length;
            }

            using var fs = disk.OpenWriteTruncate(path);
            using var bw = new BinaryWriter(fs);
            using var s = new AlbionWriter(bw);
            HeaderSerdes(lengths, s);

            for (int i = 0; i < count; i++)
                s.Bytes(null, ordered[i].Item2, lengths[i]);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path))
                return new List<(int, int)>();

            using var s = new AlbionReader(new BinaryReader(disk.OpenRead(path)));
            var lengths = HeaderSerdes(null, s);
            return new List<(int, int)> { (0, lengths.Length) };
        }

        static byte[] LoadAsset(int subItem, ISerializer s)
        {
            var lengths = HeaderSerdes(null, s);
            if (subItem >= lengths.Length)
                throw new ArgumentOutOfRangeException($"Tried to load subItem {subItem} from XLD, but it only contains {lengths.Length} items.");

            long offset = s.Offset;
            offset += lengths.Where((x, i) => i < subItem).Sum();
            s.Seek(offset);
            return s.Bytes(null, null, lengths[subItem]);
        }

        static int[] HeaderSerdes(int[] lengths, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            s.Check();
            s.Begin("XldHeader");
            string magic = s.NullTerminatedString("MagicString", MagicString);
            s.Check();
            if(magic != MagicString)
                throw new FormatException("XLD file magic string not found");

            ushort objectCount = s.UInt16("ObjectCount", (ushort)(lengths?.Length ?? 0));
            s.Check();
            lengths ??= new int[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                lengths[i] = s.Int32("Length" + i, lengths[i]);
                s.Check();
            }

            s.End();
            return lengths;
        }

        static void ReadEmbedded<TContext>(
            XldCategory category,
            int firstId,
            TContext context,
            ISerializer s,
            Action<int, int, TContext, ISerializer> func)
        {
            var descriptor = s.Object("XldDescriptor", (XldDescriptor)null, XldDescriptor.Serdes);
            ApiUtil.Assert(descriptor.Category == category);
            ApiUtil.Assert(descriptor.Number == firstId / 100);

            var preheader = s.Offset;
            var lengths = HeaderSerdes(null, s);
            ApiUtil.Assert(preheader + HeaderSize(lengths.Length) == s.Offset);

            ApiUtil.Assert(lengths.Sum() + HeaderSize(lengths.Length) == descriptor.Size);
            long offset = s.Offset;
            for (int i = 0; i < 100 && i < lengths.Length; i++)
            {
                if (lengths[i] == 0)
                    continue;

                using var window = new WindowingFacadeSerializer(s, null);
                func(i + firstId, lengths[i], context, window);
                offset += lengths[i];
                ApiUtil.Assert(offset == s.Offset);
            }
        }

        static void WriteEmbedded<TContext>(
            XldCategory category,
            int firstId,
            int lastId,
            TContext context,
            ISerializer s,
            Action<int, int, TContext, ISerializer> func,
            IList<int> populatedIds)
        {
            int count = populatedIds.Where(x => x >= firstId && x <= lastId).Max() - firstId + 1;
            var descriptorOffset = s.Offset;
            var lengths = new int[count];
            s.Seek(s.Offset + XldDescriptor.SizeInBytes + HeaderSize(count));

            for (int i = 0; i < count; i++)
            {
                using var window = new WindowingFacadeSerializer(s, null);
                func(i + firstId, 0, context, window);
                lengths[i] = (int)window.Offset;
            }

            var endOffset = s.Offset;

            // Jump back to the start and write the descriptor including the total size
            s.Seek(descriptorOffset);
            var descriptor = new XldDescriptor
            {
                Category = category,
                Number = (ushort)(firstId / 100),
                Size = (uint)(lengths.Sum() + lengths.Length * 4 + 8)
            };
            s.Object("XldDescriptor", descriptor, XldDescriptor.Serdes);
            HeaderSerdes(lengths, s);
            s.Seek(endOffset);
        }

        public static void Serdes<TContext>(
            XldCategory category,
            int firstId,
            int lastId,
            TContext context,
            ISerializer s,
            Action<int, int, TContext, ISerializer> func,
            IList<int> populatedIds)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if(lastId < firstId) throw new ArgumentOutOfRangeException(nameof(lastId));

            s.Object($"{category}.{firstId}-{lastId}", s2 =>
            {
                if (s2.IsReading())
                    ReadEmbedded(category, firstId, context, s2, func);
                else
                    WriteEmbedded(category, firstId, lastId, context, s2, func, populatedIds);
            });
        }
    }
}
