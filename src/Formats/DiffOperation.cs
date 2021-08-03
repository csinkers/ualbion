using System;

namespace UAlbion.Formats
{
    public readonly struct DiffOperation : IEquatable<DiffOperation>
    {
        readonly int _length;
        readonly int _offset;

        DiffOperation(int offset, int length) { _offset = offset; _length = length; }
        public static DiffOperation Insert(byte value) => new(value, 0);
        public static DiffOperation Copy(int offset, int length) => new(offset, length);

        public bool IsCopy => _length > 0;
        public byte Value => _length == 0 ? (byte)_offset : throw new InvalidOperationException("Tried to access Value of a copy DiffOperation");
        public int Offset => _length > 0 ? _offset : throw new InvalidOperationException("Tried to access Offset of an insert DiffOperation");
        public int Length => _length > 0 ? _length : throw new InvalidOperationException("Tried to access Length of an insert DiffOperation");
        public override string ToString() => IsCopy ? $"C({Offset},{Length})" : $"I{Value}";
        public override bool Equals(object obj) => obj is DiffOperation d && Equals(d);
        public bool Equals(DiffOperation other) => other._length == _length && other._offset == _offset;
        public static bool operator ==(DiffOperation left, DiffOperation right) => left.Equals(right);
        public static bool operator !=(DiffOperation left, DiffOperation right) => !(left == right);
        public override int GetHashCode()
        {
            var l1 = (uint)_length & 0xffff0000 >> 16;
            var l2 = (uint)_length & 0xffff;
            return (int)((l1 | l2) ^ (uint)_offset);
        }
    }
}