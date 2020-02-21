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

        public void CheckEntireLengthRead()
        {
            Check();
            if(_br.BaseStream.Position < _maxOffset)
                Debug.Fail("Failed to read entire buffer");
        }

        public void Seek(long newOffset)
        {
            _br.BaseStream.Seek(newOffset, SeekOrigin.Begin);
            _offset = newOffset;
        }

        public void Int8(string name, Func<sbyte> getter, Action<sbyte> setter) { setter(_br.ReadSByte()); _offset += 1L; }
        public void Int16(string name, Func<short> getter, Action<short> setter) { setter(_br.ReadInt16()); _offset += 2L; }
        public void Int32(string name, Func<int> getter, Action<int> setter) { setter(_br.ReadInt32()); _offset += 4L; }
        public void Int64(string name, Func<long> getter, Action<long> setter) { setter(_br.ReadInt64()); _offset += 8L; }
        public void UInt8(string name, Func<byte> getter, Action<byte> setter) { setter(_br.ReadByte()); _offset += 1L; }
        public void UInt16(string name, Func<ushort> getter, Action<ushort> setter) { setter(_br.ReadUInt16()); _offset += 2L; }
        public void UInt32(string name, Func<uint> getter, Action<uint> setter) { setter(_br.ReadUInt32()); _offset += 4L; }
        public void UInt64(string name, Func<ulong> getter, Action<ulong> setter) { setter(_br.ReadUInt64()); _offset += 8L; }

        public void EnumU8<T>(string name, Func<T> getter, Action<T> setter, Func<T, (byte, string)> infoFunc) where T : Enum
        {
            setter((T)(object)_br.ReadByte()); _offset += 1L;
        }

        public void EnumU16<T>(string name, Func<T> getter, Action<T> setter, Func<T, (ushort, string)> infoFunc) where T : Enum
        {
            setter((T)(object)_br.ReadUInt16()); _offset += 2L;
        }

        public void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> infoFunc) where T : Enum
        {
            setter((T)(object)_br.ReadUInt32()); _offset += 4L;
        }

        public void Guid(string name, Func<Guid> getter, Action<Guid> setter)
        {
            setter(new Guid(_br.ReadBytes(16)));
            _offset += 16L;
        }

        public void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = _br.ReadBytes(n);
            setter(v);
            _offset += v.Length;
        }

        public void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int n, string comment)
        {
            var v = _br.ReadBytes(n);
            setter(v);
            _offset += v.Length;
        }

        public void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = _br.ReadBytes(n);
            setter(v);
            _offset += v.Length;
        }

        public void NullTerminatedString(string name, Func<string> getter, Action<string> setter)
        {
            var bytes = new List<byte>();
            for(;;)
            {
               var b = _br.ReadByte();
               if (b == 0)
                   break;
               bytes.Add(b);
            }

            var str = FormatUtil.BytesTo850String(bytes.ToArray());
            setter(str);
        }

        public void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length)
        {
            var str = FormatUtil.BytesTo850String(_br.ReadBytes(length));
            setter(str);
            _offset += length;
            Debug.Assert(_offset == _br.BaseStream.Position);
        }

        public void RepeatU8(string name, byte v, int length)
        {
            var bytes = _br.ReadBytes(length);
            foreach(var b in bytes)
                if (b != v) throw new InvalidOperationException("Unexpected value found in repeating byte pattern");
            _offset += length;
        }

        public void Meta(string name, Action<ISerializer> deserializer, Action<ISerializer> serializer) => deserializer(this);

        public void Dynamic<TTarget>(TTarget target, string propertyName)
        {
            var serializer = SerializationInfo.Get<TTarget>(propertyName);
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
