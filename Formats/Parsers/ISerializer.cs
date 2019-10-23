using System;

namespace UAlbion.Formats.Parsers
{
    public interface ISerializer
    {
        SerializerMode Mode { get; }
        long Offset { get; } // For recording offsets to be overwritten later
        void Comment(string comment); // Only affects annotating writers
        void Indent(); // Only affects annotating writers
        void Unindent(); // Only affects annotating writers
        void NewLine(); // Only affects annotating writers
        void Seek(long offset); // For overwriting pre-recorded offsets

        void Int8(string name, Func<sbyte> getter, Action<sbyte> setter);
        void Int16(string name, Func<short> getter, Action<short> setter);
        void Int32(string name, Func<int> getter, Action<int> setter);
        void Int64(string name, Func<long> getter, Action<long> setter);
        void UInt8(string name, Func<byte> getter, Action<byte> setter);
        void UInt16(string name, Func<ushort> getter, Action<ushort> setter);
        void UInt32(string name, Func<uint> getter, Action<uint> setter);
        void UInt64(string name, Func<ulong> getter, Action<ulong> setter);

        void Guid(string name, Func<Guid> getter, Action<Guid> setter);
        void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int length);
        void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int length);
        void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int length, string coment);
        void NullTerminatedString(string name, Func<string> getter, Action<string> setter);
        void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length);

        void RepeatU8(string name, byte value, int count); // Either writes a block of padding or verifies the consistency of one while reading
        void Meta(string name, Action<ISerializer> reader, Action<ISerializer> writer); // name serializer deserializer

        void EnumU8<T>(string name, Func<T> getter, Action<T> setter, Func<T, (byte, string)> getMeta) where T : Enum;
        void EnumU16<T>(string name, Func<T> getter, Action<T> setter, Func<T, (ushort, string)> getMeta) where T : Enum;
        void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> getMeta) where T : Enum;
    }
}
