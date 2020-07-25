using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public struct InventoryId : IConvertible, IEquatable<InventoryId>
    {
        public InventoryId(PartyCharacterId id) : this(InventoryType.Player, (ushort)id) { }
        public InventoryId(ChestId id) : this(InventoryType.Chest, (ushort)id) { }
        public InventoryId(MerchantId id) : this(InventoryType.Merchant, (ushort)id) { }
        public InventoryId(InventoryType type, ushort id)
        {
            Type = type;
            Id = id;
        }

        public InventoryType Type { get; }
        public ushort Id { get; }

        public override string ToString() => Type switch
        {
            InventoryType.Player => ((PartyCharacterId)Id).ToString(),
            InventoryType.Chest => ((ChestId)Id).ToString(),
            InventoryType.Merchant => ((MerchantId)Id).ToString(),
            InventoryType.CombatLoot => "CombatLoot",
            _ => Id.ToString()
        };

        public static explicit operator int(InventoryId id) => (int)id.Type << 16 | id.Id;
        public static explicit operator InventoryId(int id)
            => new InventoryId(
                (InventoryType)((id & 0x7fff0000) >> 16),
                (ushort)(id & 0xffff));

        public static explicit operator InventoryId(PartyCharacterId id) => new InventoryId(InventoryType.Player,   (ushort)id);
        public static explicit operator InventoryId(ChestId id)          => new InventoryId(InventoryType.Chest,    (ushort)id);
        public static explicit operator InventoryId(MerchantId id)       => new InventoryId(InventoryType.Merchant, (ushort)id);

        public static explicit operator InventoryId(AssetKey key) => key.Type switch
        {
            AssetType.PartyMember => new InventoryId(InventoryType.Player, (ushort)key.Id),
            AssetType.ChestData => new InventoryId(InventoryType.Chest, (ushort)key.Id),
            AssetType.MerchantData => new InventoryId(InventoryType.Merchant, (ushort)key.Id),
            _ => throw new InvalidCastException($"Can't cast asset of type {key.Type} to inventory id")
        };

        public int ToInt32(IFormatProvider provider) => (int)this;
        public TypeCode GetTypeCode() => throw new NotImplementedException();
        public bool ToBoolean(IFormatProvider provider) => throw new NotImplementedException();
        public byte ToByte(IFormatProvider provider) => throw new NotImplementedException();
        public char ToChar(IFormatProvider provider) => throw new NotImplementedException();
        public DateTime ToDateTime(IFormatProvider provider) => throw new NotImplementedException();
        public decimal ToDecimal(IFormatProvider provider) => throw new NotImplementedException();
        public double ToDouble(IFormatProvider provider) => throw new NotImplementedException();
        public short ToInt16(IFormatProvider provider) => throw new NotImplementedException();
        public long ToInt64(IFormatProvider provider) => throw new NotImplementedException();
        public sbyte ToSByte(IFormatProvider provider) => throw new NotImplementedException();
        public float ToSingle(IFormatProvider provider) => throw new NotImplementedException();
        public string ToString(IFormatProvider provider) => throw new NotImplementedException();
        public ushort ToUInt16(IFormatProvider provider) => throw new NotImplementedException();
        public uint ToUInt32(IFormatProvider provider) => throw new NotImplementedException();
        public ulong ToUInt64(IFormatProvider provider) => throw new NotImplementedException();
        public object ToType(Type conversionType, IFormatProvider provider) => throw new NotImplementedException();


        public static bool operator ==(InventoryId x, InventoryId y) => x.Equals(y);
        public static bool operator !=(InventoryId x, InventoryId y) => !(x == y);
        public bool Equals(InventoryId other) => Type == other.Type && Id == other.Id;
        public override bool Equals(object obj) => obj is InventoryId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}