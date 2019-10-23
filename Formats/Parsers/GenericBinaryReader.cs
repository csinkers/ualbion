using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Parsers
{
    class GenericBinaryReader : ISerializer
    {
        readonly BinaryReader br;
        long offset;

        public GenericBinaryReader(BinaryReader br)
        {
            this.br = br;
            offset = 0L;
        }

        public SerializerMode Mode => SerializerMode.Reading;
        public void Comment(string msg) { }
        public void Indent() { }
        public void Unindent() { }
        public void NewLine() { }
        public long Offset
        {
            get
            {
                Debug.Assert(offset == br.BaseStream.Position);
                return offset;
            }
        }

        public void Seek(long newOffset)
        {
            br.BaseStream.Seek(newOffset, SeekOrigin.Begin);
            offset = newOffset;
        }

        public void Int8(string name, Func<sbyte> getter, Action<sbyte> setter) { setter(br.ReadSByte()); offset += 1L; }
        public void Int16(string name, Func<short> getter, Action<short> setter) { setter(br.ReadInt16()); offset += 2L; }
        public void Int32(string name, Func<int> getter, Action<int> setter) { setter(br.ReadInt32()); offset += 4L; }
        public void Int64(string name, Func<long> getter, Action<long> setter) { setter(br.ReadInt64()); offset += 8L; }
        public void UInt8(string name, Func<byte> getter, Action<byte> setter) { setter(br.ReadByte()); offset += 1L; }
        public void UInt16(string name, Func<ushort> getter, Action<ushort> setter) { setter(br.ReadUInt16()); offset += 2L; }
        public void UInt32(string name, Func<uint> getter, Action<uint> setter) { setter(br.ReadUInt32()); offset += 4L; }
        public void UInt64(string name, Func<ulong> getter, Action<ulong> setter) { setter(br.ReadUInt64()); offset += 8L; }

        public void EnumU8<T>(string name, Func<T> getter, Action<T> setter, Func<T, (byte, string)> infoFunc) where T : Enum
        {
            setter((T)(object)br.ReadByte()); offset += 1L;
        }

        public void EnumU16<T>(string name, Func<T> getter, Action<T> setter, Func<T, (ushort, string)> infoFunc) where T : Enum
        {
            setter((T)(object)br.ReadUInt16()); offset += 2L;
        }

        public void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> infoFunc) where T : Enum
        {
            setter((T)(object)br.ReadUInt32()); offset += 4L;
        }

        public void Guid(string name, Func<Guid> getter, Action<Guid> setter)
        {
            setter(new Guid(br.ReadBytes(16)));
            offset = offset + 16L;
        }

        public void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset = offset + v.Length;
        }

        public void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int n, string comment)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset = offset + v.Length;
        }

        public void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset = offset + v.Length;
        }

        public void NullTerminatedString(string name, Func<string> getter, Action<string> setter)
        {
            IEnumerable<char> Aux()
            {
                for (;;)
                {
                    var bytes = br.ReadBytes(2);
                    offset += 2L;
                    var codePoint = System.Text.Encoding.Unicode.GetChars(bytes)[0];
                    if (codePoint == (char) 0) yield break;
                    yield return codePoint;
                }
            }

            var str = new string(Aux().ToArray());
            setter(str);
        }

        public void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length)
        {
            IEnumerable<char> aux(int remaining)
            {
                for (; ; )
                {
                    var bytes = br.ReadBytes(2);
                    var codePoint = System.Text.Encoding.Unicode.GetChars(bytes)[0];
                    if (codePoint == (char)0) yield break;
                    if (remaining > 2)
                        br.ReadBytes(remaining - 2);
                    else
                        yield return codePoint;
                    if (remaining < 0)
                        throw new InvalidOperationException("Non-even string length passed to readUnicodeString");

                    if (remaining == 0) yield break;
                    remaining -= 2;
                }
            }

            var str = new string(aux(length).ToArray());
            setter(str);
            offset = offset + length;
            Debug.Assert(offset == br.BaseStream.Position);
        }

        public void RepeatU8(string name, byte v, int length)
        {
            var bytes = br.ReadBytes(length);
            foreach(var b in bytes)
                if (b != v) throw new InvalidOperationException("Unexpected value found in repeating byte pattern");
            offset = offset + length;
        }

        public void Meta(string name, Action<ISerializer> serializer, Action<ISerializer> deserializer) => deserializer(this);
    }
}