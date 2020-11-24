using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetMapping
    {
        static readonly ThreadLocal<AssetMapping> ThreadLocalGlobal = new ThreadLocal<AssetMapping>(() => new AssetMapping());
        static readonly AssetMapping TrueGlobal = new AssetMapping();
        static readonly AssetType[] AllAssetTypes =
            typeof(AssetType)
                .GetEnumValues()
                .OfType<AssetType>()
                .ToArray();
#if DEBUG
        static readonly AssetType[] UnmappedTypes =
            AllAssetTypes
            .Where(x =>
                typeof(AssetType)
                .GetMember(x.ToString())[0]
                .GetCustomAttributes(typeof(UnmappedAttribute), false)
                .FirstOrDefault() != null
            ).ToArray();
#endif

        // The global mapping, should always be used apart from when loading/saving assets.
        // Always built dynamically based on the current set of active mods
        public static AssetMapping Global => GlobalIsThreadLocal ? ThreadLocalGlobal.Value : TrueGlobal;
        public static bool GlobalIsThreadLocal { get; set; } // Set to true for unit tests.

        readonly struct Range
        {
            public Range(int from, int to) { From = from; To = to; }
            public int From { get; }
            public int To { get; }
        }

        class EnumInfo
        {
            [JsonIgnore] readonly string _enumTypeString;
            [JsonIgnore] public Type EnumType { get; set; }
            [JsonIgnore] public string EnumTypeString => _enumTypeString ?? EnumType.AssemblyQualifiedName;
            [JsonConverter(typeof(ToStringJsonConverter))] public AssetType AssetType { get; set; }
            public int EnumMin { get; set; }
            public int EnumMax { get; set; }
            public int Offset { get; set; }
            [JsonIgnore] public int MappedMin => EnumMin + Offset;
            [JsonIgnore] public int MappedMax => EnumMax + Offset;
            public Range[] Ranges { get; set; }

            public EnumInfo() { } // For deserialisation

            public EnumInfo(string typeString, AssetType assetType, int? lastMax)
            {
                _enumTypeString = typeString;
                EnumType = Type.GetType(typeString) ?? throw new InvalidOperationException();
                if (!EnumType.IsEnum)
                    throw new InvalidOperationException($"Tried to register type {EnumType} as an asset identifier for assets of type {assetType}, but it is not an enum.");

                AssetType = assetType;

                var values =
                    Enum.GetValues(EnumType)
                    .Cast<object>()
                    .Select(Convert.ToInt32)
                    .OrderBy(x => x)
                    .ToArray();

                EnumMin = values.Min();
                EnumMax = values.Max();
                Offset = (lastMax ?? (EnumMin-1)) + 1 - EnumMin;
                Ranges = values.Aggregate(new List<Range>(), (acc, x) =>
                    {
                        if (acc.Count == 0) acc.Add(new Range(x, x));
                        else if (acc[acc.Count - 1].To == x - 1) acc[acc.Count - 1] = new Range(acc[acc.Count - 1].From, x);
                        else acc.Add(new Range(x, x));
                        return acc;
                    })
                    .Select(x => new Range(x.From + Offset, x.To + Offset))
                    .ToArray();
            }
        }

        // Note: All mappings should be registered very early on in the program lifecycle
        // and then never changed. If this is followed, then no locking is necessary for
        // concurrent readers. When a mod needs to change its mapping, all assets should
        // be loaded from disk using the old mapping into the global mapping, then saved
        // to disk using the new mapping.

        readonly List<EnumInfo>[] _byAssetType = 
            Enumerable.Repeat(0, 256)
                .Select(_ => new List<EnumInfo>())
                .ToArray(); // Keyed by AssetType, a byte enum

        readonly Dictionary<Type, EnumInfo> _byEnumType = new Dictionary<Type, EnumInfo>();
        readonly Dictionary<string, List<(EnumInfo, int)>> _byName = new Dictionary<string, List<(EnumInfo, int)>>();

        public AssetMapping() {}
        AssetMapping(Dictionary<Type, EnumInfo> byEnumType)
        {
            _byEnumType = byEnumType;
            foreach (var grouping in byEnumType.GroupBy(x => x.Value.AssetType))
            {
                var typeMapping = _byAssetType[(byte)grouping.Key];
                var ordered = grouping.OrderBy(x => x.Value.MappedMin);
                typeMapping.AddRange(ordered.Select(kvp => kvp.Value));
            }
        }

        /// <summary>
        /// Convert a run-time AssetId to its unambiguous enum representation.
        /// </summary>
        /// <param name="id">The asset id to convert</param>
        /// <returns>The type of the enumeration and numerical value of the enum member corresponding to the asset id</returns>
        public (string, int) IdToEnumString(AssetId id)
        {
            foreach (var info in _byAssetType[(byte)id.Type])
            {
                if (info.MappedMax < id.Id)
                    continue;

                ApiUtil.Assert(id.Id <= info.MappedMax, $"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
                return (info.EnumTypeString, id.Id - info.Offset);
            }

#if DEBUG
            if (!UnmappedTypes.Contains(id.Type))
                ApiUtil.Assert($"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
#endif
            return (null, id.ToInt32());
        }

        /// <summary>
        /// Convert a run-time AssetId to its unambiguous enum representation.
        /// </summary>
        /// <param name="id">The asset id to convert</param>
        /// <returns>The type of the enumeration and numerical value of the enum member corresponding to the asset id</returns>
        public (Type, int) IdToEnum(AssetId id)
        {
            foreach (var info in _byAssetType[(byte)id.Type])
            {
                if (info.MappedMax < id.Id)
                    continue;

                ApiUtil.Assert(id.Id <= info.MappedMax, $"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
                return (info.EnumType, id.Id - info.Offset);
            }

#if DEBUG
            if (!UnmappedTypes.Contains(id.Type))
                ApiUtil.Assert($"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
#endif
            return (null, id.ToInt32());
        }

        /// <summary>
        /// Get the name of a given AssetId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string IdToName(AssetId id)
        {
            var (enumType, enumValue) = IdToEnum(id);
            return enumType == null
                ? enumValue == 0 ? "None" : $"{id.Type}.{enumValue}"
                : enumType.Name + "." + Enum.GetName(enumType, enumValue);
        }

        /// <summary>
        /// Convert an enum to an asset id for internal use.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="id">The value of the enumeration to convert</param>
        /// <returns></returns>
        public AssetId EnumToId<T>(T id) where T : unmanaged, Enum
        {
            if (!_byEnumType.TryGetValue(typeof(T), out var info))
                throw new ArgumentOutOfRangeException($"Type {typeof(T)} is not currently mapped.");

            unsafe
            {
                int enumValue =
                      sizeof(T) == 1 ? Unsafe.As<T, byte>(ref id)
                    : sizeof(T) == 2 ? Unsafe.As<T, ushort>(ref id)
                    : sizeof(T) == 4 ? Unsafe.As<T, int>(ref id)
                    : throw new InvalidOperationException("Type {typeof(T)} is of non-enum type, or has an unsupported underlying type");

                ApiUtil.Assert(enumValue >= info.EnumMin && enumValue <= info.EnumMax, $"Enum value {id} of type {typeof(T)} with a value of {enumValue} falls outside the mapped range");
                return new AssetId(info.AssetType, enumValue + info.Offset);
            }
        }

        public AssetId EnumToId((Type, int) value) => EnumToId(value.Item1, value.Item2);
        public AssetId EnumToId(Type enumType, int enumValue)
        {
            if (enumType == null)
                return AssetId.FromInt32(enumValue);

            if (!_byEnumType.TryGetValue(enumType, out var info))
                throw new ArgumentOutOfRangeException($"Type {enumType} is not currently mapped.");

            if (enumValue < info.EnumMin)
            {
                if (enumValue == 0)
                    return AssetId.None;
                throw new ArgumentOutOfRangeException($"Value {enumValue} of type {enumType} is out of range (below minimum value {info.EnumMin})");
            }

            if (enumValue > info.EnumMax)
                throw new ArgumentOutOfRangeException($"Value {enumValue} of type {enumType} is out of range (above maximum value {info.EnumMin})");

            return new AssetId(info.AssetType, enumValue + info.Offset);
        }

        public AssetMapping Clear()
        {
            _byEnumType.Clear();
            foreach(var mapping in _byAssetType)
                mapping.Clear();
            return this;
        }

        /// <summary>
        /// Register an enum type whose values should be mapped into the global asset namespaces.
        /// </summary>
        /// <param name="enumType">The enum type to map</param>
        /// <param name="assetType">The type of asset that the enum identifies</param>
        public AssetMapping RegisterAssetType(Type enumType, AssetType assetType) => RegisterAssetType(enumType?.AssemblyQualifiedName, assetType);

        /// <summary>
        /// Register an enum type whose values should be mapped into the global asset namespaces.
        /// </summary>
        /// <param name="enumName">The enum type to map</param>
        /// <param name="assetType">The type of asset that the enum identifies</param>
        public AssetMapping RegisterAssetType(string enumName, AssetType assetType)
        {
            if (enumName == null) throw new ArgumentNullException(nameof(enumName));

            var mapping = _byAssetType[(byte)assetType];
            var info = new EnumInfo(enumName, assetType, mapping.LastOrDefault()?.MappedMax);
            if (_byEnumType.ContainsKey(info.EnumType))
                return this;

            mapping.Add(info);
            _byEnumType[info.EnumType] = info;

            foreach(var value in
                Enum.GetValues(info.EnumType)
                .Cast<object>()
                .Select(x => (x.ToString(), Convert.ToInt32(x, CultureInfo.InvariantCulture))))
            {
                if (!_byName.TryGetValue(value.Item1, out var entries))
                {
                    entries = new List<(EnumInfo, int)>();
                    _byName[value.Item1] = entries;
                }
                entries.Add((info, value.Item2 + info.Offset));
            }

            return this;
        }

        public IEnumerable<AssetId> EnumeratAssetsOfType(AssetType type)
        {
            foreach (var info in  _byAssetType[(byte)type]) // Nested for-loops go brrr
                foreach (var range in info.Ranges)
                    for (int i = range.From; i <= range.To; i++)
                        yield return new AssetId(type, i);
        }

        public string Serialize(JsonSerializerSettings settings) => JsonConvert.SerializeObject(_byEnumType, settings);
        public static AssetMapping Deserialize(string json)
        {
            var m = new AssetMapping(JsonConvert.DeserializeObject<Dictionary<Type, EnumInfo>>(json));
            foreach (var kvp in m._byEnumType)
                kvp.Value.EnumType = kvp.Key;
            return m;
        }

        public void MergeFrom(AssetMapping other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var info in other._byAssetType.SelectMany(x => x))
            {
                if (!_byEnumType.ContainsKey(info.EnumType))
                    RegisterAssetType(info.EnumTypeString, info.AssetType);
            }
        }

        public AssetId Parse(string s, AssetType[] validTypes) // pass null for validTypes to allow any type
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(nameof(s));
            s = s.Trim();
            int index = s.LastIndexOf('.');
            var valueName = index == -1 ? s : s.Substring(index + 1);
            var typeName = index == -1 ? null : s.Substring(0, index);
            // TODO: Use typeName to resolve ambiguous matches

            if (!_byName.TryGetValue(valueName, out var matches))
                return ParseNumeric(s, typeName, valueName, validTypes);

            AssetId result = new AssetId(AssetType.Unknown);
            foreach (var match in matches)
            {
                if (validTypes != null && !validTypes.Contains(match.Item1.AssetType))
                    continue;

                if (!string.IsNullOrEmpty(typeName)
                    && !match.Item1.EnumType.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)
                    && !match.Item1.AssetType.ToString().Equals(typeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (result.Type != AssetType.Unknown)
                {
                    var candidates = string.Join(", ",
                        matches
                            .Where(x => validTypes == null || validTypes.Contains(x.Item1.AssetType))
                            .Select(x => x.Item1.EnumType.Name));

                    throw new InvalidOperationException($"Could not unambiguously parse \"{s}\" as an asset id. Candidate types: {candidates}");
                }

                result = new AssetId(match.Item1.AssetType, match.Item2);
            }

            if (result.Type == AssetType.Unknown)
                throw new KeyNotFoundException($"Could not parse \"{s}\" as an asset id enum");

            return result;
        }

        AssetId ParseNumeric(string s, string typeName, string valueName, AssetType[] validTypes) // pass null for validTypes to allow any type
        {
            AssetId result = new AssetId(AssetType.Unknown);
            if (int.TryParse(valueName, out var intValue))
            {
                validTypes ??= AllAssetTypes;
                foreach (var assetType in validTypes)
                {
                    if (!string.IsNullOrEmpty(typeName)
                        && !assetType.ToString().Equals(typeName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (result.Type != AssetType.Unknown)
                    {
                        var candidates = string.Join(", ", validTypes.Select(x => x.ToString()));
                        throw new InvalidOperationException($"Could not unambiguously parse \"{s}\" as an asset id. Candidate types: {candidates}");
                    }

                    result = new AssetId(assetType, intValue);
                }
            }

            if (result.Type == AssetType.Unknown)
                throw new KeyNotFoundException($"Could not parse \"{s}\" as an asset id enum");

            return result;
        }
    }
}