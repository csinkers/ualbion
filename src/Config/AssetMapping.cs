using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetMapping
    {
        static readonly ThreadLocal<AssetMapping> ThreadLocalGlobal = 
            new(() => new AssetMapping());
        static readonly AssetMapping TrueGlobal = new();
        static readonly AssetType[] AllAssetTypes =
            typeof(AssetType)
                .GetEnumValues()
                .OfType<AssetType>()
                .ToArray();

        static T GetAttribute<T>(object x) where T : Attribute =>
            (T)x.GetType()
                .GetMember(x.ToString())[0]
                .GetCustomAttributes(typeof(T), false)
                .FirstOrDefault();

#if DEBUG
        static readonly AssetType[] UnmappedTypes = AllAssetTypes.Where(x => GetAttribute<UnmappedAttribute>(x) != null).ToArray();
#endif

        // The global mapping, should always be used apart from when loading/saving assets.
        // Always built dynamically based on the current set of active mods
        public static AssetMapping Global => GlobalIsThreadLocal ? ThreadLocalGlobal.Value : TrueGlobal;
        public static bool GlobalIsThreadLocal { get; set; } // Set to true for unit tests.

        readonly struct Range
        {
            [JsonConstructor]
            public Range(int from, int to) { From = from; To = to; }
            public int From { get; }
            public int To { get; }
            public override string ToString() => $"{From}:{To}";
        }

        class EnumInfo
        {
            [JsonIgnore] readonly string _enumTypeString;
            [JsonIgnore] public Type EnumType { get; set; }
            [JsonIgnore] public string EnumTypeString => _enumTypeString ?? EnumType.AssemblyQualifiedName;
            public AssetType AssetType { get; set; }
            public int EnumMin { get; set; }
            public int EnumMax { get; set; }
            public int Offset { get; set; }
            [JsonIgnore] public int MappedMin => EnumMin + Offset;
            [JsonIgnore] public int MappedMax => EnumMax + Offset;
            public Range[] Ranges { get; set; }

            public EnumInfo() { } // For deserialisation
            public override string ToString() => $"{EnumType.Name}@{Offset} = {AssetType}";

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
                Offset = (lastMax ?? (EnumMin - 1)) + 1 - EnumMin;
                Ranges = values.Aggregate(new List<Range>(), (acc, x) =>
                    {
                        if (acc.Count == 0) acc.Add(new Range(x, x));
                        else if (acc[^1].To == x - 1) acc[^1] = new Range(acc[^1].From, x);
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
        // Adding a new mapping won't invalidate existing ids, but removing one would
        // likely require flushing and reloading all assets to avoid misinterpretations.
        readonly List<EnumInfo>[] _byAssetType =
            Enumerable.Repeat(0, 256)
                .Select(_ => new List<EnumInfo>())
                .ToArray(); // Keyed by AssetType, a byte enum

        readonly Dictionary<Type, EnumInfo> _byEnumType = new();
        readonly Dictionary<string, List<(EnumInfo, int)>> _byName = new();
        readonly Dictionary<AssetId, (AssetId, ushort)> _stringLookup = new();

        public AssetMapping() { }
        AssetMapping(Dictionary<Type, EnumInfo> byEnumType)
        {
            _byEnumType = byEnumType ?? throw new ArgumentNullException(nameof(byEnumType));
            foreach (var grouping in byEnumType.GroupBy(x => x.Value.AssetType))
            {
                var typeMapping = _byAssetType[(byte)grouping.Key];
                var ordered = grouping.OrderBy(x => x.Value.MappedMin);
                typeMapping.AddRange(ordered.Select(kvp => kvp.Value));
            }

            foreach (var info in byEnumType.Values)
                RegisterNames(info);
        }

        public bool IsGlobal => Global == this;

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
            if (id == AssetId.None) // Special case to keep things tidy
                return "None";

            var (enumType, enumValue) = IdToEnum(id);
            if (enumType == null)
                return $"{id.Type}.{id.Id}";

            var enumName = Enum.GetName(enumType, enumValue);
            if (!string.IsNullOrEmpty(enumName))
                return enumType.Name + "." + enumName;

            return $"{enumType.Name}.{enumValue}";
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
                    : throw new InvalidOperationException($"Type {typeof(T)} is of non-enum type, or has an unsupported underlying type");

                ApiUtil.Assert(enumValue >= info.EnumMin && enumValue <= info.EnumMax, $"Enum value {id} of type {typeof(T)} with a value of {enumValue} falls outside the mapped range");
                return new AssetId(info.AssetType, enumValue + info.Offset);
            }
        }

        public AssetId EnumToId((Type, int) value) => EnumToId(value.Item1, value.Item2);
        public AssetId EnumToId(Type enumType, int enumValue)
        {
            if (enumType == null)
            {
                var id = AssetId.FromInt32(enumValue);
                return id.Id == 0 ? AssetId.None : id;
            }

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

        public AssetId EnumToId(Type enumType, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!_byEnumType.TryGetValue(enumType, out var enumInfo))
                throw new FormatException($"Could not parse a value of type \"{enumType}\", as it has not been registered in the asset mapping");

            if (int.TryParse(value, out int intValue))
                return new AssetId(enumInfo.AssetType, intValue);

            if (!_byName.TryGetValue(value.ToUpperInvariant(), out var matches))
                throw new FormatException($"Could not parse id \"{value}\" as \"{enumType}\": no such value");

            var match = matches.FirstOrDefault(x => x.Item1 == enumInfo);
            if (match == (null, 0))
                throw new FormatException($"Could not parse id \"{value}\" as \"{enumType}\": no such value");

            return new AssetId(match.Item1.AssetType, match.Item2);
        }

        public AssetMapping Clear()
        {
            _byEnumType.Clear();
            _byName.Clear();

            foreach (var mapping in _byAssetType)
                mapping.Clear();
            return this;
        }

        /// <summary>
        /// Register an enum type whose values should be mapped into the global asset namespaces.
        /// </summary>
        /// <param name="enumType">The enum type to map</param>
        /// <param name="assetType">The type of asset that the enum identifies</param>
        public AssetMapping RegisterAssetType(Type enumType, AssetType assetType)
            => RegisterAssetType(enumType?.AssemblyQualifiedName, assetType);

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
            RegisterNames(info);

            return this;
        }

        void RegisterNames(EnumInfo info)
        {
            foreach (var value in
                Enum.GetValues(info.EnumType)
                .Cast<object>()
                .Select(x => (x.ToString(), Convert.ToInt32(x, CultureInfo.InvariantCulture))))
            {
                var key = value.Item1.ToUpperInvariant();
                if (!_byName.TryGetValue(key, out var entries))
                {
                    entries = new List<(EnumInfo, int)>();
                    _byName[key] = entries;
                }
                entries.Add((info, value.Item2 + info.Offset));
            }
        }

        public AssetMapping RegisterStringRedirect(Type enumType, AssetId target, int min, int max, int offset)
        {
            foreach (var id in EnumerateAssetsOfType(enumType))
            {
                var (_, numeric) = IdToEnum(id);
                if (numeric < min || numeric > max)
                    continue;

                _stringLookup[id] = (target, (ushort)(offset + numeric - min));
            }
            return this;
        }

        public void ConsistencyCheck()
        {
            (string, string ) Describe(AssetType assetType)
            {
                var sbBasic = new StringBuilder();
                var sbFull = new StringBuilder();
                foreach (var info in _byAssetType[(byte)assetType])
                {
                    var basic = $"O:{info.Offset} {string.Join(" ", info.Ranges.Select(x => x.ToString()))}";
                    sbBasic.AppendLine(basic);
                    sbFull.AppendLine($"      {basic} ({info.EnumTypeString})");
                }

                return (sbBasic.ToString(), sbFull.ToString());
            }


            var issues = new List<string>();
            foreach (var assetType in AllAssetTypes)
            {
                var iso = GetAttribute<IsomorphicToAttribute>(assetType);
                if (iso != null)
                {
                    var (basicA, fullA) = Describe(assetType);
                    var (basicB, fullB) = Describe(iso.Type);
                    if (string.Equals(basicA, basicB, StringComparison.Ordinal)) continue;
                    issues.Add($"  The mapping of {assetType} should be isomorphic to {iso.Type}, but it is not:");
                    issues.Add($"    {assetType} mapping:");
                    issues.Add(fullA);
                    issues.Add($"    {iso.Type} mapping:");
                    issues.Add(fullB);
                }
            }

            if (issues.Count > 0)
            {
                throw new InvalidOperationException(
                    "AssetMapping constraints violated: " 
                    + Environment.NewLine 
                    + string.Join(Environment.NewLine, issues));
            }
        }

        public (AssetId, ushort)? TextIdToStringId(AssetId id) 
            => _stringLookup.ContainsKey(id) 
                ? _stringLookup[id] 
                : ((AssetId, ushort)?)null;

        public IEnumerable<AssetId> EnumerateAssetsOfType(AssetType type)
        {
            foreach (var info in _byAssetType[(byte)type]) // Nested for-loops go brrr
                foreach (var range in info.Ranges)
                    for (int i = range.From; i <= range.To; i++)
                        yield return new AssetId(type, i);
        }

        public IEnumerable<AssetId> EnumerateAssetsOfType(Type type)
        {
            if (!_byEnumType.TryGetValue(type, out var info))
                yield break;

            foreach (var range in info.Ranges)
                for (int i = range.From; i <= range.To; i++)
                    yield return new AssetId(info.AssetType, i);
        }

        public string Serialize() => JsonUtil.Serialize(
            _byEnumType.ToDictionary(
                x => x.Key.AssemblyQualifiedName,
                x => x.Value));

        public static AssetMapping Deserialize(byte[] json)
        {
            var stringKeyed = JsonUtil.Deserialize<Dictionary<string, EnumInfo>>(json);
            var typeKeyed = stringKeyed.ToDictionary(
                x => Type.GetType(x.Key),
                x => x.Value);

            foreach (var kvp in typeKeyed)
                kvp.Value.EnumType = kvp.Key;

            return new AssetMapping(typeKeyed);
        }

        public void MergeFrom(AssetMapping other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var info in other._byAssetType.SelectMany(x => x))
            {
                if (!_byEnumType.ContainsKey(info.EnumType))
                    RegisterAssetType(info.EnumTypeString, info.AssetType);
            }

            foreach (var redirect in other._stringLookup)
                _stringLookup[redirect.Key] = redirect.Value;
        }

        public AssetId Parse(string s, AssetType[] validTypes) // pass null for validTypes to allow any type
        {
            if (string.IsNullOrEmpty(s))
                return AssetId.None;

            s = s.Trim().ToUpperInvariant();
            int index = s.LastIndexOf('.');
            var valueName = index == -1 ? s : s.Substring(index + 1);
            var typeName = index == -1 ? null : s.Substring(0, index);

            // Special case so we don't have ugly "None.0"s everywhere.
            if (typeName == null && string.Equals(s, "NONE", StringComparison.Ordinal)) return AssetId.None;

            return _byName.TryGetValue(valueName, out var matches)
                ? ParseTextual(s, typeName, matches, validTypes)
                : ParseNumeric(s, typeName, valueName, validTypes);
        }

        static AssetId ParseTextual(string s, string typeName, IList<(EnumInfo, int)> matches, AssetType[] validTypes)
        {
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

                    throw new FormatException($"Could not unambiguously parse \"{s}\" as an asset id. Candidate types: {candidates}");
                }

                result = new AssetId(match.Item1.AssetType, match.Item2);
            }

            if (result.Type == AssetType.Unknown)
                throw new FormatException($"Could not parse \"{s}\" as an asset id enum");

            return result;
        }

        static AssetId ParseNumeric(string s, string typeName, string valueName, AssetType[] validTypes) // pass null for validTypes to allow any type
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
                        throw new FormatException($"Could not unambiguously parse \"{s}\" as an asset id. Candidate types: {candidates}");
                    }

                    result = new AssetId(assetType, intValue);
                }
            }

            return result;
        }
    }
}
