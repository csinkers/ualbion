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
        bool IsComplete(); // Ensure offset matches stream position

        sbyte Int8(string name, sbyte existing);
        short Int16(string name, short existing);
        int Int32(string name, int existing);
        long Int64(string name, long existing);
        byte UInt8(string name, byte existing);
        ushort UInt16(string name, ushort existing);
        uint UInt32(string name, uint existing);
        ulong UInt64(string name, ulong existing);

        T EnumU8<T>(string name, T existing) where T : struct, Enum;
        T EnumU16<T>(string name, T existing) where T : struct, Enum;
        T EnumU32<T>(string name, T existing) where T : struct, Enum;

        TMemory Transform<TPersistent, TMemory>(string name, TMemory existing, Func<string, TPersistent, TPersistent> serializer, IConverter<TPersistent, TMemory> converter);
        /*
            var persistent = converter.ToPersistent(existing);
            persistent = serializer(persistent);
            return converter.ToMemory(persistent);
         */

        Guid Guid(string name, Guid existing);
        byte[] ByteArray(string name, byte[] existing, int length);
        byte[] ByteArrayHex(string name, byte[] existing, int length);
        byte[] ByteArray2(string name, byte[] existing, int length, string coment);
        string NullTerminatedString(string name, string existing);
        string FixedLengthString(string name, string existing, int length);

        void RepeatU8(string name, byte value, int count); // Either writes a block of padding or verifies the consistency of one while reading
        void Meta(string name, Action<ISerializer> reader, Action<ISerializer> writer); // name serializer deserializer
        T Meta<T>(string name, T existing, Func<int, T, ISerializer, T> serdes);

        void Dynamic<TTarget>(TTarget target, string propertyName);
        void List<TTarget>(IList<TTarget> list, int count, Func<int, TTarget, ISerializer, TTarget> serdes) where TTarget : class;
        void List<TTarget>(IList<TTarget> list, int count, int offset, Func<int, TTarget, ISerializer, TTarget> serializer) where TTarget : class;
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
/*
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
                */
    }
}
