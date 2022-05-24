using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats;

public class EmptySerializer : ISerializer
{
    public SerializerFlags Flags => SerializerFlags.Read;
    public long Offset => 0;
    public long BytesRemaining => 0;
    public void Dispose() { } 
    public void Comment(string comment) { } 
    public void Begin(string name = null) { } 
    public void End() { } 
    public void NewLine() { } 
    public void Seek(long offset) => throw new NotSupportedException();
    public void Check() { } 
    public void Assert(bool condition, string message) { } 
    public bool IsComplete() => true;
    public void Pad(int bytes) => throw new NotSupportedException();
    public sbyte Int8(string name, sbyte value, sbyte defaultValue = 0) => throw new NotSupportedException();
    public short Int16(string name, short value, short defaultValue = 0) => throw new NotSupportedException();
    public int Int32(string name, int value, int defaultValue = 0) => throw new NotSupportedException();
    public long Int64(string name, long value, long defaultValue = 0) => throw new NotSupportedException();
    public byte UInt8(string name, byte value, byte defaultValue = 0) => throw new NotSupportedException();
    public ushort UInt16(string name, ushort value, ushort defaultValue = 0) => throw new NotSupportedException();
    public uint UInt32(string name, uint value, uint defaultValue = 0) => throw new NotSupportedException();
    public ulong UInt64(string name, ulong value, ulong defaultValue = 0) => throw new NotSupportedException();
    public T EnumU8<T>(string name, T value) where T : unmanaged, Enum => throw new NotSupportedException();
    public T EnumU16<T>(string name, T value) where T : unmanaged, Enum => throw new NotSupportedException();
    public T EnumU32<T>(string name, T value) where T : unmanaged, Enum => throw new NotSupportedException();
    public T Transform<TNumeric, T>(string name, T value, Func<string, TNumeric, ISerializer, TNumeric> serializer, IConverter<TNumeric, T> converter) => throw new NotSupportedException();
    public T TransformEnumU8<T>(string name, T value, IConverter<byte, T> converter) => throw new NotSupportedException();
    public T TransformEnumU16<T>(string name, T value, IConverter<ushort, T> converter) => throw new NotSupportedException();
    public T TransformEnumU32<T>(string name, T value, IConverter<uint, T> converter) => throw new NotSupportedException();
    public Guid Guid(string name, Guid value) => throw new NotSupportedException();
    public byte[] Bytes(string name, byte[] value, int length) => throw new NotSupportedException();
    public string NullTerminatedString(string name, string value) => throw new NotSupportedException();
    public string FixedLengthString(string name, string value, int length) => throw new NotSupportedException();
    public void RepeatU8(string name, byte value, int count) => throw new NotSupportedException();
    public T Object<T>(string name, T value, Func<int, T, ISerializer, T> serdes) => throw new NotSupportedException();
    public T Object<T, TContext>(string name, T value, TContext context, Func<int, T, TContext, ISerializer, T> serdes) => throw new NotSupportedException();
    public void Object(string name, Action<ISerializer> serdes) => throw new NotSupportedException();
    public IList<TTarget> List<TTarget>(string name, IList<TTarget> list, int count, Func<int, TTarget, ISerializer, TTarget> serdes, Func<int, IList<TTarget>> initialiser = null) => throw new NotSupportedException();
    public IList<TTarget> List<TTarget>(string name, IList<TTarget> list, int count, int offset, Func<int, TTarget, ISerializer, TTarget> serdes, Func<int, IList<TTarget>> initialiser = null) => throw new NotSupportedException();
    public IList<TTarget> List<TTarget, TContext>(string name, IList<TTarget> list, TContext context, int count, Func<int, TTarget, TContext, ISerializer, TTarget> serdes,
        Func<int, IList<TTarget>> initialiser = null) =>
        throw new NotSupportedException();

    public IList<TTarget> List<TTarget, TContext>(string name, IList<TTarget> list, TContext context, int count, int offset, Func<int, TTarget, TContext, ISerializer, TTarget> serdes,
        Func<int, IList<TTarget>> initialiser = null) =>
        throw new NotSupportedException();
}