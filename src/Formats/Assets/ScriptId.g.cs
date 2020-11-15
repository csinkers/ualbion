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
    [TypeConverter(typeof(ScriptIdConverter))]
    public struct ScriptId : IEquatable<ScriptId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public ScriptId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Script))
                throw new ArgumentOutOfRangeException($"Tried to construct a ScriptId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a ScriptId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public ScriptId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Script))
                throw new ArgumentOutOfRangeException($"Tried to construct a ScriptId with a type of {Type}");
        }
        public ScriptId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Script))
                throw new ArgumentOutOfRangeException($"Tried to construct a ScriptId with a type of {Type}");
        }

        public static ScriptId From<T>(T id) where T : unmanaged, Enum => (ScriptId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static ScriptId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new ScriptId(AssetType.Script, disk));
            return (ScriptId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static ScriptId SerdesU8(string name, ScriptId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static ScriptId SerdesU16(string name, ScriptId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static ScriptId None => new ScriptId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Script };
        public static ScriptId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(ScriptId id) => new AssetId(id._value);
        public static implicit operator ScriptId(AssetId id) => new ScriptId((uint)id);
        public static explicit operator uint(ScriptId id) => id._value;
        public static explicit operator int(ScriptId id) => unchecked((int)id._value);
        public static explicit operator ScriptId(int id) => new ScriptId(id);
        public static implicit operator ScriptId(UAlbion.Base.Script id) => ScriptId.From(id);

        public static ScriptId ToScriptId(int id) => new ScriptId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(ScriptId x, ScriptId y) => x.Equals(y);
        public static bool operator !=(ScriptId x, ScriptId y) => !(x == y);
        public static bool operator ==(ScriptId x, AssetId y) => x.Equals(y);
        public static bool operator !=(ScriptId x, AssetId y) => !(x == y);
        public bool Equals(ScriptId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }

    public class ScriptIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? ScriptId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}