using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Parsers
{
    class GenericBinaryWriter : ISerializer
    {
        readonly BinaryWriter bw;
        long offset;

        public GenericBinaryWriter(BinaryWriter bw)
        {
            this.bw = bw;
        }

        public SerializerMode Mode => SerializerMode.Writing;
        public void Comment(string msg) { }
        public void Indent() { }
        public void Unindent() { }
        public void NewLine() { }
        public long Offset
        {
            get
            {
                Debug.Assert(offset == bw.BaseStream.Position);
                return offset;
            }
        }

        public void Seek(long newOffset)
        {
            bw.Seek((int)newOffset, SeekOrigin.Begin);
            offset = newOffset;
        }

        public void Int8(string name, Func<sbyte> getter, Action<sbyte> setter) { bw.Write(getter()); offset += 1L; }
        public void Int16(string name, Func<short> getter, Action<short> setter) { bw.Write(getter()); offset += 2L; }
        public void Int32(string name, Func<int> getter, Action<int> setter) { bw.Write(getter()); offset += 4L; }
        public void Int64(string name, Func<long> getter, Action<long> setter) { bw.Write(getter()); offset += 8L; }
        public void UInt8(string name, Func<byte> getter, Action<byte> setter) { bw.Write(getter()); offset += 1L; }
        public void UInt16(string name, Func<ushort> getter, Action<ushort> setter) { bw.Write(getter()); offset += 2L; }
        public void UInt32(string name, Func<uint> getter, Action<uint> setter) { bw.Write(getter()); offset += 4L; }
        public void UInt64(string name, Func<ulong> getter, Action<ulong> setter) { bw.Write(getter()); offset += 8L; }
        public void EnumU8<T>(string name, Func<T> getter, Action<T> setter, Func<T, (byte, string)> infoFunc) where T : Enum
        {
            bw.Write(infoFunc(getter()).Item1); offset += 1L;
        }
        public void EnumU16<T>(string name, Func<T> getter, Action<T> setter, Func<T, (ushort, string)> infoFunc) where T : Enum
        {
            bw.Write(infoFunc(getter()).Item1); offset += 2L;
        }
        public void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> infoFunc) where T : Enum
        {
            bw.Write(infoFunc(getter()).Item1); offset += 4L;
        }

        public void Guid(string name, Func<Guid> getter, Action<Guid> setter)
        {
            var v = getter();
            bw.Write(v.ToByteArray());
            offset += 16L;
        }

        public void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = getter();
            bw.Write(v);
            offset += v.Length;
        }
        public void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int n, string comment)
        {
            var v = getter();
            bw.Write(v);
            offset += v.Length;
        }
        public void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = getter();
            bw.Write(v);
            offset += v.Length;
        }

        public void NullTerminatedString(string name, Func<string> getter, Action<string> setter)
        {
            var v = getter();
            var bytes = FormatUtil.BytesFrom850String(v);
            bw.Write(bytes);
            bw.Write((byte)0);
            offset += bytes.Length + 1; // add 2 bytes for the null terminator
        }
        public void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length)
        {
            var v = getter();
            var bytes = FormatUtil.BytesFrom850String(v);
            if (bytes.Length > length + 1) throw new InvalidOperationException("Tried to write overlength string");
            bw.Write(bytes);
            bw.Write(Enumerable.Repeat((byte)0, length - bytes.Length).ToArray());
            offset += length; // Pad out to the full length
        }

        public void RepeatU8(string name, byte v, int length)
        {
            bw.Write(Enumerable.Repeat(v, length).ToArray());
            offset += length;
        }

        public void Meta(string name, Action<ISerializer> serializer, Action<ISerializer> deserializer) => serializer(this);
        public void Check() { }
    }
}