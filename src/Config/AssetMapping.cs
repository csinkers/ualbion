using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetMapping
    {
        class EnumInfo
        {
            public Type EnumType { get; }
            public AssetType AssetType { get; }
            public int EnumMin { get; }
            public int EnumMax { get; }
            public int Offset { get; }
            public int MappedMin => EnumMin + Offset;
            public int MappedMax => EnumMax + Offset;
            public (int, int)[] Ranges { get; }

            public EnumInfo(Type type, AssetType assetType, int mappedMin)
            {
                EnumType = type;
                AssetType = assetType;

                var values =
                    Enum.GetValues(type)
                    .Cast<object>()
                    .Select(Convert.ToInt32)
                    .OrderBy(x => x)
                    .ToArray();

                EnumMin = values.Min();
                EnumMax = values.Max();
                Offset = mappedMin - EnumMin;
                Ranges = values.Aggregate(new List<(int, int)>(), (acc, x) =>
                {
                    if (acc.Count == 0) acc.Add((x, x));
                    else if (acc[acc.Count - 1].Item2 == x - 1) acc[acc.Count - 1] = (acc[acc.Count - 1].Item1, x);
                    else acc.Add((x, x));
                    return acc;
                }).ToArray();
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

        static readonly ThreadLocal<AssetMapping> _threadLocalGlobal = new ThreadLocal<AssetMapping>(() => new AssetMapping());
        static readonly AssetMapping _trueGlobal = new AssetMapping();

        // The global mapping, should always be used apart from when loading/saving assets.
        // Always built dynamically based on the current set of active mods
        public static AssetMapping Global => GlobalIsThreadLocal ? _threadLocalGlobal.Value : _trueGlobal;
        public static bool GlobalIsThreadLocal { get; set; } // Set to true for unit tests.

        /// <summary>
        /// Convert a run-time AssetId to its unambiguous enum representation.
        /// </summary>
        /// <param name="id">The asset id to convert</param>
        /// <returns>The type of the enumeration and numerical value of the enum member corresponding to the asset id</returns>
        public (Type, int) IdToEnum(AssetId id)
        {
            foreach (var info in _byAssetType[(byte)id.Type])
            {
                if (info.MappedMin > id.Id)
                    continue;

                ApiUtil.Assert(id.Id <= info.MappedMax, $"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
                return (info.EnumType, id.Id + info.Offset);
            }

            ApiUtil.Assert($"AssetId ({id.Type}, {id.Id}) is outside the mapped range.");
            return (typeof(int), id.Id);
        }

        /// <summary>
        /// Get the name of a given AssetId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string IdToName(AssetId id)
        {
            var (enumType, enumValue) = IdToEnum(id);
            return enumType == typeof(int) 
                ? $"{id.Type}.??{enumValue}??" 
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

        public AssetId EnumToId(Type enumType, int enumValue)
        {
            var info = _byEnumType[enumType];
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
        public AssetMapping RegisterAssetType(Type enumType, AssetType assetType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum)
                throw new InvalidOperationException($"Tried to register type {enumType} as an asset identifier for assets of type {assetType}, but it is not an enum.");

            if (_byEnumType.ContainsKey(enumType))
                throw new InvalidOperationException($"Tried to register type {enumType} as an asset identifier for assets of type {assetType}, but it is already registered.");

            var mapping = _byAssetType[(byte)assetType];
            var info = new EnumInfo(enumType, assetType, (mapping.LastOrDefault()?.MappedMax ?? -1) + 1);
            mapping.Add(info);
            _byEnumType[enumType] = info;
            return this;
        }

        public IEnumerable<AssetId> EnumeratAssetsOfType(AssetType type)
        {
            foreach (var info in  _byAssetType[(byte)type]) // Nested for loops go brrr
                foreach (var (from, to) in info.Ranges)
                    for (int i = from; i < to; i++)
                        yield return new AssetId(type, i);
        }
    }
}