using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.Parsers
{
    public class GenericBinaryReader : ISerializer
    {
        readonly BinaryReader _br;
        readonly long _maxOffset;
        long _offset;

        public GenericBinaryReader(BinaryReader br, long maxLength)
        {
            _br = br;
            _offset = br.BaseStream.Position;
            _maxOffset = _offset + maxLength;
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
                Debug.Assert(_offset == _br.BaseStream.Position);
                if(_br.BaseStream.Position > _maxOffset)
                    Debug.Fail("Buffer overrun in binary reader");
                return _offset;
            }
        }

        public void Check()
        {
            Debug.Assert(_offset == _br.BaseStream.Position);
            if (_br.BaseStream.Position > _maxOffset)
                Debug.Fail("Buffer overrun in binary reader");
        }

        public bool IsComplete() => _br.BaseStream.Position >= _maxOffset;

        public void Seek(long newOffset)
        {
            _br.BaseStream.Seek(newOffset, SeekOrigin.Begin);
            _offset = newOffset;
        }

        public sbyte Int8(string name, sbyte existing) { _offset += 1L; return _br.ReadSByte(); }
        public short Int16(string name, short existing) { _offset += 2L; return _br.ReadInt16(); }
        public int Int32(string name, int existing) { _offset += 4L; return _br.ReadInt32(); }
        public long Int64(string name, long existing) { _offset += 8L; return _br.ReadInt64(); }
        public byte UInt8(string name, byte existing) { _offset += 1L; return _br.ReadByte(); }
        public ushort UInt16(string name, ushort existing) { _offset += 2L; return _br.ReadUInt16(); }
        public uint UInt32(string name, uint existing) { _offset += 4L; return _br.ReadUInt32(); }
        public ulong UInt64(string name, ulong existing) { _offset += 8L; return _br.ReadUInt64(); }
        public T EnumU8<T>(string name, T existing) where T : struct, Enum { _offset += 1L; return (T)(object)_br.ReadByte(); }
        public T EnumU16<T>(string name, T existing) where T : struct, Enum { _offset += 2L; return (T)(object)_br.ReadUInt16(); }
        public T EnumU32<T>(string name, T existing) where T : struct, Enum { _offset += 4L; return (T)(object)_br.ReadUInt32(); }
        public Guid Guid(string name, Guid existing) { _offset += 16L; return new Guid(_br.ReadBytes(16)); }

        public byte[] ByteArray(string name, byte[] existing, int n)
        {
            var v = _br.ReadBytes(n);
            _offset += v.Length;
            return v;
        }

        public byte[] ByteArray2(string name, byte[] existing, int n, string comment)
        {
            var v = _br.ReadBytes(n);
            _offset += v.Length;
            return v;
        }

        public byte[] ByteArrayHex(string name, byte[] existing, int n)
        {
            var v = _br.ReadBytes(n);
            _offset += v.Length;
            return v;
        }

        public string NullTerminatedString(string name, string existing)
        {
            var bytes = new List<byte>();
            for (;;)
            {
                var b = _br.ReadByte();
               if (b == 0)
                   break;
               bytes.Add(b);
            }

            return FormatUtil.BytesTo850String(bytes.ToArray());
        }

        public string FixedLengthString(string name, string existing, int length)
        {
            var str = FormatUtil.BytesTo850String(_br.ReadBytes(length));
            _offset += length;
            Debug.Assert(_offset == _br.BaseStream.Position);
            return str;
        }

        public void RepeatU8(string name, byte v, int length)
        {
            var bytes = _br.ReadBytes(length);
            foreach(var b in bytes)
                if (b != v) throw new InvalidOperationException("Unexpected value found in repeating byte pattern");
            _offset += length;
        }

        public TMemory Transform<TPersistent, TMemory>(string name, TMemory existing, Func<string, TPersistent, TPersistent> serializer, IConverter<TPersistent, TMemory> converter) =>
            converter.ToMemory(serializer(name, converter.ToPersistent(existing)));

        public void Meta(string name, Action<ISerializer> deserializer, Action<ISerializer> serializer) => deserializer(this);
        public T Meta<T>(string name, T existing, Func<int, T, ISerializer, T> serdes) => serdes(0, existing, this);
/*
        public void Dynamic<TTarget>(TTarget target, string propertyName)
        {
            SerializationInfo<TTarget> serializer = SerializationInfo.Get<TTarget>(propertyName);
            switch (serializer)
            {
                case SerializationInfo<TTarget, byte>   s: s.Setter(target, _br.ReadByte()); break;
                case SerializationInfo<TTarget, sbyte>  s: s.Setter(target, _br.ReadSByte()); break;
                case SerializationInfo<TTarget, ushort> s: s.Setter(target, _br.ReadUInt16()); break;
                case SerializationInfo<TTarget, short>  s: s.Setter(target, _br.ReadInt16()); break;
                case SerializationInfo<TTarget, uint>   s: s.Setter(target, _br.ReadUInt32()); break;
                case SerializationInfo<TTarget, int>    s: s.Setter(target, _br.ReadInt32()); break;
                case SerializationInfo<TTarget, ulong>  s: s.Setter(target, _br.ReadUInt64()); break;
                case SerializationInfo<TTarget, long>   s: s.Setter(target, _br.ReadInt64()); break;
                default: throw new InvalidOperationException($"Tried to serialize unexpected type {serializer.Type}");
            }
            _offset += serializer.Size;
        }*/

        public void List<TTarget>(IList<TTarget> list, int count, Func<int, TTarget, ISerializer, TTarget> serdes) where TTarget : class
        {
            for (int i = 0; i < count; i++)
            {
                var x = serdes(i, null, this);
                list.Add(x);
            }
        }

        public void List<TTarget>(IList<TTarget> list, int count, int offset, Func<int, TTarget, ISerializer, TTarget> serdes) where TTarget : class
        {
            while(list.Count < offset)
                list.Add(null);

            for (int i = offset; i < offset + count; i++)
            {
                var x = serdes(i, null, this);
                list.Add(x);
            }
        }
    }
}
