using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Parsers
{
    public class GenericBinaryWriter : ISerializer
    {
        readonly BinaryWriter _bw;
        long _offset;

        public GenericBinaryWriter(BinaryWriter bw)
        {
            _bw = bw;
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
                Debug.Assert(_offset == _bw.BaseStream.Position);
                return _offset;
            }
        }

        public void Seek(long newOffset)
        {
            _bw.Seek((int)newOffset, SeekOrigin.Begin);
            _offset = newOffset;
        }

        public sbyte Int8(string name, sbyte existing)     { _bw.Write(existing); _offset += 1L; return existing; }
        public short Int16(string name, short existing)    { _bw.Write(existing); _offset += 2L; return existing; }
        public int Int32(string name, int existing)        { _bw.Write(existing); _offset += 4L; return existing; }
        public long Int64(string name, long existing)      { _bw.Write(existing); _offset += 8L; return existing; }
        public byte UInt8(string name, byte existing)      { _bw.Write(existing); _offset += 1L; return existing; }
        public ushort UInt16(string name, ushort existing) { _bw.Write(existing); _offset += 2L; return existing; }
        public uint UInt32(string name, uint existing)     { _bw.Write(existing); _offset += 4L; return existing; }
        public ulong UInt64(string name, ulong existing)   { _bw.Write(existing); _offset += 8L; return existing; }
        public T EnumU8<T>(string name, T existing) where T : struct, Enum
        {
            _bw.Write((byte)(object)existing);
            _offset += 1L;
            return existing;
        }

        public T EnumU16<T>(string name, T existing) where T : struct, Enum
        {
            _bw.Write((ushort)(object)existing);
            _offset += 2L;
            return existing;
        }

        public T EnumU32<T>(string name, T existing) where T : struct, Enum
        {
            _bw.Write((uint)(object)existing);
            _offset += 4L;
            return existing;
        }

        public Guid Guid(string name, Guid existing)
        {
            var v = existing;
            _bw.Write(v.ToByteArray());
            _offset += 16L;
            return existing;
        }

        public byte[] ByteArray(string name, byte[] existing, int n)
        {
            var v = existing;
            _bw.Write(v);
            _offset += v.Length;
            return existing;
        }
        public byte[] ByteArray2(string name, byte[] existing, int n, string comment)
        {
            var v = existing;
            _bw.Write(v);
            _offset += v.Length;
            return existing;
        }
        public byte[] ByteArrayHex(string name, byte[] existing, int n)
        {
            var v = existing;
            _bw.Write(v);
            _offset += v.Length;
            return existing;
        }

        public string NullTerminatedString(string name, string existing)
        {
            var v = existing;
            var bytes = FormatUtil.BytesFrom850String(v);
            _bw.Write(bytes);
            _bw.Write((byte)0);
            _offset += bytes.Length + 1; // add 2 bytes for the null terminator
            return existing;
        }

        public string FixedLengthString(string name, string existing, int length)
        {
            var v = existing;
            var bytes = FormatUtil.BytesFrom850String(v);
            if (bytes.Length > length + 1) throw new InvalidOperationException("Tried to write overlength string");
            _bw.Write(bytes);
            _bw.Write(Enumerable.Repeat((byte)0, length - bytes.Length).ToArray());
            _offset += length; // Pad out to the full length
            return existing;
        }

        public void RepeatU8(string name, byte v, int length)
        {
            _bw.Write(Enumerable.Repeat(v, length).ToArray());
            _offset += length;
        }

        public TMemory Transform<TPersistent, TMemory>(string name, TMemory existing, Func<string, TPersistent, TPersistent> serializer, IConverter<TPersistent, TMemory> converter) =>
            converter.ToMemory(serializer(name, converter.ToPersistent(existing)));

        public void Meta(string name, Action<ISerializer> serializer, Action<ISerializer> deserializer) => serializer(this);
        public T Meta<T>(string name, T existing, Func<int, T, ISerializer, T> serdes) => serdes(0, existing, this);

        public void Check() { }
        public bool IsComplete() => false;
/*
        public void Dynamic<TTarget>(TTarget target, string propertyName)
        {
            var serializer = SerializationInfo.Get<TTarget>(propertyName);
            switch (serializer)
            {
                case SerializationInfo<TTarget, byte>   s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, sbyte>  s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, ushort> s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, short>  s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, uint>   s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, int>    s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, ulong>  s: _bw.Write(s.Getter(target)); break;
                case SerializationInfo<TTarget, long>   s: _bw.Write(s.Getter(target)); break;
                default: throw new InvalidOperationException($"Tried to serialize unexpected type {serializer.Type}");
            }
            _offset += serializer.Size;
        } */

        public void List<TTarget>(IList<TTarget> list, int count, Func<int, TTarget, ISerializer, TTarget> serializer) where TTarget : class
        {
            for (int i = 0; i < count; i++)
                serializer(i, list[i], this);
        }

        public void List<TTarget>(IList<TTarget> list, int count, int offset, Func<int, TTarget, ISerializer, TTarget> serializer) where TTarget : class
        {
            for (int i = offset; i < count + offset; i++)
                serializer(i, list[i], this);
        }
    }
}
