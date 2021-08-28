using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats
{
    // ReSharper disable once UnusedType.Global
    public sealed class BreakpointFacadeSerializer : ISerializer // For debugging unintentional overwrites
    {
        readonly ISerializer _s;
        public (long from, long to)? BreakRange { get; set; }
        public BreakpointFacadeSerializer(ISerializer s) => _s = s ?? throw new ArgumentNullException(nameof(s));
        public void Dispose() {}
        public SerializerFlags Flags => _s.Flags;
        public long Offset => _s.Offset;
        public long BytesRemaining => _s.BytesRemaining;
        public void Comment(string comment) => _s.Comment(comment);
        public void Begin(string name = null) => _s.Begin(name);
        public void End() => _s.End();
        public void NewLine() => _s.NewLine();

        void CheckV(Action action)
        {
            var start = _s.Offset;
            action();

            if (BreakRange == null)
                return;

            var finish = _s.Offset;
            if (start <= BreakRange.Value.to && finish >= BreakRange.Value.from)
                throw new InvalidOperationException($"HIT BREAKPOINT FOR ({BreakRange.Value.from}, {BreakRange.Value.to}), START: {start} FINISH:{finish}");
        }

        T CheckT<T>(Func<T> func)
        {
            var start = _s.Offset;
            var result = func();

            if (BreakRange != null)
            {
                var finish = _s.Offset;
                if (start <= BreakRange.Value.to && finish >= BreakRange.Value.@from)
                    throw new InvalidOperationException($"HIT BREAKPOINT FOR ({BreakRange.Value.@from}, {BreakRange.Value.to}), START: {start} FINISH:{finish}");
            }

            return result;
        }

        public void Seek(long offset) => _s.Seek(offset);
        public void Check() => _s.Check();
        public void Assert(bool condition, string message) => _s.Assert(condition, message);
        public bool IsComplete() => _s.IsComplete();
        public void Pad(int bytes) => CheckV(() => _s.Pad(bytes));
#pragma warning disable CA1720 // Identifier contains type name
        public sbyte Int8(string name, sbyte value, sbyte defaultValue = 0) => CheckT(() => _s.Int8(name, value, defaultValue));
        public short Int16(string name, short value, short defaultValue = 0) => CheckT(() => _s.Int16(name, value, defaultValue));
        public int Int32(string name, int value, int defaultValue = 0) => CheckT(() => _s.Int32(name, value, defaultValue));
        public long Int64(string name, long value, long defaultValue = 0) => CheckT(() => _s.Int64(name, value, defaultValue));
        public byte UInt8(string name, byte value, byte defaultValue = 0) => CheckT(() => _s.UInt8(name, value, defaultValue));
        public ushort UInt16(string name, ushort value, ushort defaultValue = 0) => CheckT(() => _s.UInt16(name, value, defaultValue));
        public uint UInt32(string name, uint value, uint defaultValue = 0) => CheckT(() => _s.UInt32(name, value, defaultValue));
        public ulong UInt64(string name, ulong value, ulong defaultValue = 0) => CheckT(() => _s.UInt64(name, value, defaultValue));
        public T EnumU8<T>(string name, T value) where T : unmanaged, Enum => CheckT(() => _s.EnumU8(name, value));
        public T EnumU16<T>(string name, T value) where T : unmanaged, Enum => CheckT(() => _s.EnumU16(name, value));
        public T EnumU32<T>(string name, T value) where T : unmanaged, Enum => CheckT(() => _s.EnumU32(name, value));
        public T Transform<TNumeric, T>(string name, T value, Func<string, TNumeric, ISerializer, TNumeric> serializer, IConverter<TNumeric, T> converter) 
            => CheckT(() => _s.Transform(name, value, serializer, converter));
        public T TransformEnumU8<T>(string name, T value, IConverter<byte, T> converter) => CheckT(() => _s.TransformEnumU8(name, value, converter));
        public T TransformEnumU16<T>(string name, T value, IConverter<ushort, T> converter) => CheckT(() => _s.TransformEnumU16(name, value, converter));
        public T TransformEnumU32<T>(string name, T value, IConverter<uint, T> converter) => CheckT(() => _s.TransformEnumU32(name, value, converter));
        public Guid Guid(string name, Guid value) => CheckT(() => _s.Guid(name, value));
        public byte[] Bytes(string name, byte[] value, int length) => CheckT(() => _s.Bytes(name, value, length));
        public string NullTerminatedString(string name, string value) => CheckT(() => _s.NullTerminatedString(name, value));
        public string FixedLengthString(string name, string value, int length) => CheckT(() => _s.FixedLengthString(name, value, length));
        public void RepeatU8(string name, byte value, int count) => CheckV(() => _s.RepeatU8(name, value, count));
        public T Object<T>(string name, T value, Func<int, T, ISerializer, T> serdes) => CheckT(() => _s.Object(name, value, serdes));
        public T Object<T, TContext>(string name, T value, TContext context, Func<int, T, TContext, ISerializer, T> serdes) 
            => CheckT(() => _s.Object(name, value, context, serdes));
        public void Object(string name, Action<ISerializer> serdes) => CheckV(() => _s.Object(name, serdes));
#pragma warning restore CA1720 // Identifier contains type name
        public IList<TTarget> List<TTarget>(
            string name,
            IList<TTarget> list,
            int count,
            Func<int, TTarget, ISerializer, TTarget> serdes,
            Func<int, IList<TTarget>> initialiser = null) 
            => CheckT(() => _s.List(name, list, count, serdes, initialiser));

        public IList<TTarget> List<TTarget>(
            string name,
            IList<TTarget> list,
            int count,
            int offset,
            Func<int, TTarget, ISerializer, TTarget> serdes,
            Func<int, IList<TTarget>> initialiser = null) 
            => CheckT(() => _s.List(name, list, count, offset, serdes, initialiser));

        public IList<TTarget> List<TTarget, TContext>(
            string name,
            IList<TTarget> list,
            TContext context,
            int count,
            Func<int, TTarget, TContext, ISerializer, TTarget> serdes,
            Func<int, IList<TTarget>> initialiser = null) 
            => CheckT(() => _s.List(name, list, context, count, serdes, initialiser));

        public IList<TTarget> List<TTarget, TContext>(
            string name,
            IList<TTarget> list,
            TContext context,
            int count,
            int offset,
            Func<int, TTarget, TContext, ISerializer, TTarget> serdes,
            Func<int, IList<TTarget>> initialiser = null) 
            => CheckT(() => _s.List(name, list, context, count, offset, serdes, initialiser));
    }
}