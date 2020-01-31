using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    class GenericBinaryReader : ISerializer
    {
        readonly BinaryReader br;
        long offset;

        public GenericBinaryReader(BinaryReader br)
        {
            this.br = br;
            offset = br.BaseStream.Position;
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
            setter((T)(object)(int)br.ReadUInt16()); offset += 2L;
        }

        public void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> infoFunc) where T : Enum
        {
            setter((T)(object)br.ReadUInt32()); offset += 4L;
        }

        public void Guid(string name, Func<Guid> getter, Action<Guid> setter)
        {
            setter(new Guid(br.ReadBytes(16)));
            offset += 16L;
        }

        public void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset += v.Length;
        }

        public void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int n, string comment)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset += v.Length;
        }

        public void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = br.ReadBytes(n);
            setter(v);
            offset += v.Length;
        }

        public void NullTerminatedString(string name, Func<string> getter, Action<string> setter)
        {
            var bytes = new List<byte>();
            for(;;)
            {
               var b = br.ReadByte();
               if (b == 0)
                   break;
               bytes.Add(b);
            }

            var str = FormatUtil.BytesTo850String(bytes.ToArray());
            setter(str);
        }

        public void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length)
        {
            var str = FormatUtil.BytesTo850String(br.ReadBytes(length));
            setter(str);
            offset += length;
            Debug.Assert(offset == br.BaseStream.Position);
        }

        public void RepeatU8(string name, byte v, int length)
        {
            var bytes = br.ReadBytes(length);
            foreach(var b in bytes)
                if (b != v) throw new InvalidOperationException("Unexpected value found in repeating byte pattern");
            offset += length;
        }

        public void Meta(string name, Action<ISerializer> serializer, Action<ISerializer> deserializer) => deserializer(this);

        public void Check()
        {
            Debug.Assert(offset == br.BaseStream.Position);
        }

        public void Dynamic<TTarget>(TTarget target, string propertyName)
        {
            var serializer = SerializationInfo.Get<TTarget>(propertyName);
            switch (serializer)
            {
                case SerializationInfo<TTarget, byte>   s: s.Setter(target, br.ReadByte()); break;
                case SerializationInfo<TTarget, sbyte>  s: s.Setter(target, br.ReadSByte()); break;
                case SerializationInfo<TTarget, ushort> s: s.Setter(target, br.ReadUInt16()); break;
                case SerializationInfo<TTarget, short>  s: s.Setter(target, br.ReadInt16()); break;
                case SerializationInfo<TTarget, uint>   s: s.Setter(target, br.ReadUInt32()); break;
                case SerializationInfo<TTarget, int>    s: s.Setter(target, br.ReadInt32()); break;
                case SerializationInfo<TTarget, ulong>  s: s.Setter(target, br.ReadUInt64()); break;
                case SerializationInfo<TTarget, long>   s: s.Setter(target, br.ReadInt64()); break;
                default: throw new InvalidOperationException($"Tried to serialize unexpected type {serializer.Type}");
            }
            offset += serializer.Size;
        }

        public void List<TTarget>(IList<TTarget> list, int count, Action<TTarget, ISerializer> serializer, Func<TTarget> constructor)
        {
            for (int i = 0; i < count; i++)
            {
                var og = constructor();
                serializer(og, this);
                list.Add(og);
            }
        }
    }
}
