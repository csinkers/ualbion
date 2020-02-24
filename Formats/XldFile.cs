using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats
{
    public class XldFile : IDisposable
    {
        const string MagicString = "XLD0I";
        readonly Stream _stream;
        readonly int[] _objectOffsets;
        public string Filename { get; }
        int ObjectCount => _objectOffsets.Length - 1;

        public XldFile(string filename)
        {
            Filename = filename;
            _stream = File.OpenRead(filename);

            using var br = new BinaryReader(_stream, Encoding.Default, true);
            var headerBytes = Encoding.ASCII.GetBytes(MagicString);
            var actualHeader = br.ReadBytes(headerBytes.Length);
            if(!actualHeader.SequenceEqual(headerBytes))
                throw new FormatException("XLD file magic string not found");

            byte terminator = br.ReadByte();
            Debug.Assert(terminator == 0);

            int objectCount = br.ReadUInt16();
            _objectOffsets = new int[objectCount + 1];
            int offset = (int)_stream.Position + 4 * objectCount;

            for (int i = 0; i < objectCount; i++)
            {
                _objectOffsets[i] = offset;
                int length = br.ReadInt32();
                offset += length;
            }

            Debug.Assert(offset == _stream.Length);
            _objectOffsets[objectCount] = offset;
        }

        public BinaryReader GetReaderForObject(int objectIndex, out int length)
        {
            if(objectIndex >= ObjectCount)
                throw new FileNotFoundException($"Tried to request object {objectIndex} from {Filename}, but it only contains {ObjectCount} items.");

            _stream.Seek(_objectOffsets[objectIndex], SeekOrigin.Begin);
            length = _objectOffsets[objectIndex + 1] - _objectOffsets[objectIndex];
            return new BinaryReader(_stream, Encoding.Default, true);
        }

        public void Dispose() => _stream?.Dispose();
    }

    public class XldLoader
    {
        const string MagicString = "XLD0I";
        public int HeaderSize(int itemCount) => MagicString.Length + 3 + 4 * itemCount;
        public int[] Serdes(int[] lengths, ISerializer s)
        {
            s.Check();
            string magic = s.NullTerminatedString("MagicString", MagicString);
            s.Check();
            if(magic != MagicString)
                throw new FormatException("XLD file magic string not found");

            ushort objectCount = s.UInt16("ObjectCount", (ushort)(lengths?.Length ?? 0));
            s.Check();
            lengths ??= new int[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                lengths[i] = s.Int32(i.ToString(), lengths[i]);
                s.Check();
            }

            return lengths;
        }
    }
}
