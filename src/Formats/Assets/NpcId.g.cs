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
    [TypeConverter(typeof(NpcIdConverter))]
    public struct NpcId : IEquatable<NpcId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public NpcId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Npc))
                throw new ArgumentOutOfRangeException($"Tried to construct a NpcId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a NpcId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public NpcId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Npc))
                throw new ArgumentOutOfRangeException($"Tried to construct a NpcId with a type of {Type}");
        }
        public NpcId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Npc))
                throw new ArgumentOutOfRangeException($"Tried to construct a NpcId with a type of {Type}");
        }

        public static NpcId From<T>(T id) where T : unmanaged, Enum => (NpcId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static NpcId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new NpcId(AssetType.Npc, disk));
            return (NpcId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static NpcId SerdesU8(string name, NpcId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static NpcId SerdesU16(string name, NpcId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static NpcId None => new NpcId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Npc };
        public static NpcId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(NpcId id) => new AssetId(id._value);
        public static implicit operator NpcId(AssetId id) => new NpcId((uint)id);
        public static explicit operator uint(NpcId id) => id._value;
        public static explicit operator int(NpcId id) => unchecked((int)id._value);
        public static explicit operator NpcId(int id) => new NpcId(id);
        public static implicit operator CharacterId(NpcId id) => new CharacterId(id._value);
        public static explicit operator NpcId(CharacterId id) => new NpcId((uint)id);
        public static implicit operator NpcId(UAlbion.Base.Npc id) => NpcId.From(id);

        public static NpcId ToNpcId(int id) => new NpcId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(NpcId x, NpcId y) => x.Equals(y);
        public static bool operator !=(NpcId x, NpcId y) => !(x == y);
        public static bool operator ==(NpcId x, AssetId y) => x.Equals(y);
        public static bool operator !=(NpcId x, AssetId y) => !(x == y);
        public bool Equals(NpcId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }

    public class NpcIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? NpcId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}