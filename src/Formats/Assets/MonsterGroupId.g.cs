// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    [TypeConverter(typeof(MonsterGroupIdConverter))]
    public struct MonsterGroupId : IEquatable<MonsterGroupId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public MonsterGroupId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.MonsterGroup))
                throw new ArgumentOutOfRangeException($"Tried to construct a MonsterGroupId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a MonsterGroupId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        MonsterGroupId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.MonsterGroup))
                throw new ArgumentOutOfRangeException($"Tried to construct a MonsterGroupId with a type of {Type}");
        }

        public static MonsterGroupId From<T>(T id) where T : unmanaged, Enum => (MonsterGroupId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static MonsterGroupId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new MonsterGroupId(AssetType.MonsterGroup, disk));
            return (MonsterGroupId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static MonsterGroupId SerdesU8(string name, MonsterGroupId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static MonsterGroupId SerdesU16(string name, MonsterGroupId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static MonsterGroupId None => new MonsterGroupId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.MonsterGroup };
        public static MonsterGroupId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(MonsterGroupId id) => AssetId.FromUInt32(id._value);
        public static implicit operator MonsterGroupId(AssetId id) => new MonsterGroupId(id.ToUInt32());
        public static implicit operator MonsterGroupId(UAlbion.Base.MonsterGroup id) => MonsterGroupId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static MonsterGroupId FromInt32(int id) => new MonsterGroupId(unchecked((uint)id));
        public static MonsterGroupId FromUInt32(uint id) => new MonsterGroupId(id);
        public static bool operator ==(MonsterGroupId x, MonsterGroupId y) => x.Equals(y);
        public static bool operator !=(MonsterGroupId x, MonsterGroupId y) => !(x == y);
        public static bool operator ==(MonsterGroupId x, AssetId y) => x.Equals(y);
        public static bool operator !=(MonsterGroupId x, AssetId y) => !(x == y);
        public bool Equals(MonsterGroupId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => unchecked((int)_value);
    }

    public class MonsterGroupIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? MonsterGroupId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}