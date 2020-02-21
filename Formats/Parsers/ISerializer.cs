using System;
using System.Collections.Generic;

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
        void Check(); // Ensure offset matches stream position
        void CheckEntireLengthRead(); // Ensure offset matches stream position

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

        void Dynamic<TTarget>(TTarget target, string propertyName);

        void List<TTarget>(IList<TTarget> list, int count, Action<TTarget, ISerializer> serializer, Func<TTarget> constructor);
    }

    public static class SerializerExtensions
    {
        static ushort SwapBytes16(ushort x)
        {
            // swap adjacent 8-bit blocks
            ushort a = (ushort) ((x & 0xFF00) >> 8);
            ushort b = (ushort) ((x & 0x00FF) << 8);
            return (ushort) (a | b);
        }

        static uint SwapBytes32(uint x)
        {
            // swap adjacent 16-bit blocks
            x = ((x & 0xFFFF0000) >> 16) | ((x & 0x0000FFFF) << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        static ulong SwapBytes64(ulong x)
        {
            // swap adjacent 32-bit blocks
            x = (x >> 32) | (x << 32);
            // swap adjacent 16-bit blocks
            x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8);
        }

        public static void Int16LE(this ISerializer s, string name, Func<short> getter, Action<short> setter) =>
            s.Int16(name,
                () => (short)SwapBytes16((ushort)getter()),
                x => setter((short)SwapBytes16((ushort)x)));

        public static void Int32LE(this ISerializer s, string name, Func<int> getter, Action<int> setter) =>
            s.Int32(name,
                () => (int)SwapBytes32((uint)getter()),
                x => setter((int)SwapBytes32((uint)x)));

        public static void Int64LE(this ISerializer s, string name, Func<long> getter, Action<long> setter) =>
            s.Int64(name,
                () => (long)SwapBytes64((ulong)getter()),
                x => setter((long)SwapBytes64((ulong)x)));

        public static void UInt16LE(this ISerializer s, string name, Func<ushort> getter, Action<ushort> setter) =>
            s.UInt16(name,
                () => SwapBytes16(getter()),
                x => setter(SwapBytes16(x)));

        public static void UInt32LE(this ISerializer s, string name, Func<uint> getter, Action<uint> setter) =>
            s.UInt32(name,
                () => SwapBytes32(getter()),
                x => setter(SwapBytes32(x)));

        public static void UInt64LE(this ISerializer s, string name, Func<ulong> getter, Action<ulong> setter) =>
            s.UInt64(name,
                () => SwapBytes64(getter()),
                x => setter(SwapBytes64(x)));
    }
}
