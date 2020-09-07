﻿using System;
using System.Globalization;
using Newtonsoft.Json;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter))]
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
            InventoryType.Player => "P:" + (PartyCharacterId)Id,
            InventoryType.Chest => "C:" + (ChestId)Id,
            InventoryType.Merchant => "M:" + (MerchantId)Id,
            InventoryType.CombatLoot => "CombatLoot",
            _ => Type + ":" + Id.ToString(CultureInfo.InvariantCulture)
        };

        public string Serialise() => Type switch
        {
            InventoryType.Player => "P:" + Id,
            InventoryType.Chest => "C:" + Id,
            InventoryType.Merchant => "M:" + Id,
            InventoryType.CombatLoot => "CombatLoot",
            _ => Type + ":" + Id.ToString(CultureInfo.InvariantCulture)
        };

        public static InventoryId Parse(string s)
        {
            if (s == null || !s.Contains(":"))
                throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            var parts = s.Split(':');
            var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            switch (parts[0])
            {
                case "P": return new InventoryId((PartyCharacterId)id);
                case "C": return new InventoryId((ChestId)id);
                case "M": return new InventoryId((MerchantId)id);
                case "CombatLoot": return new InventoryId(InventoryType.CombatLoot, id);
                default: return new InventoryId((InventoryType)Enum.Parse(typeof(InventoryType), parts[0]), id);
            }
        }

        public static explicit operator int(InventoryId id) => ToInt(id);
        public static int ToInt(InventoryId id) => (int)id.Type << 16 | id.Id;

        public static explicit operator InventoryId(int id) => ToInventoryId(id);
        public static explicit operator InventoryId(PartyCharacterId id) => ToInventoryId(id);
        public static explicit operator InventoryId(ChestId id)          => ToInventoryId(id);
        public static explicit operator InventoryId(MerchantId id) => ToInventoryId(id);
        public static explicit operator InventoryId(AssetKey key) => ToInventoryId(key);
        public static explicit operator InventoryId(AssetId id) => ToInventoryId(id);
        public static InventoryId ToInventoryId(int id)
            => new InventoryId(
                (InventoryType)((id & 0x7fff0000) >> 16),
                (ushort)(id & 0xffff));

        public static InventoryId ToInventoryId(PartyCharacterId id) => new InventoryId(InventoryType.Player,   (ushort)id);
        public static InventoryId ToInventoryId(ChestId id)          => new InventoryId(InventoryType.Chest,    (ushort)id);
        public static InventoryId ToInventoryId(MerchantId id)       => new InventoryId(InventoryType.Merchant, (ushort)id);
        public static InventoryId ToInventoryId(AssetKey key) => key.Type switch
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