using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.CodeGenerator;

public class Assets
{
    public string BaseDir { get; }
    public AssetConfig AssetConfig { get; }
    public AssetIdConfig AssetIdConfig { get; }
    public Dictionary<string, string[]> ParentsByAssetId { get; }
    public Dictionary<string, string[]> AssetIdsByEnum { get; }
    public Dictionary<string, string[]> EnumsByAssetId { get; }
    public ILookup<AssetType, string> AssetIdsByType { get; }

    public Assets(IFileSystem disk, IJsonUtil jsonUtil) // Everything in this class should be treated as read-only once the constructor finishes.
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        BaseDir = ConfigUtil.FindBasePath(disk);
        var assetIdConfigPath = Path.Combine(BaseDir, @"src/Formats/AssetIdTypes.json");
        var config = GeneralConfig.Load(Path.Combine(BaseDir, "data/config.json"), BaseDir, disk, jsonUtil);

        AssetConfig = AssetConfig.Load(config.ResolvePath("$(MODS)/Base/assets.json"), AssetMapping.Global, disk, jsonUtil);
        AssetIdConfig = AssetIdConfig.Load(assetIdConfigPath, disk, jsonUtil);

        AssetIdsByType = FindAssetIdsByType(AssetIdConfig);
        ParentsByAssetId = FindAssetIdParents(AssetIdConfig, AssetIdsByType);
        AssetIdsByEnum = FindAssetIdsForEnums(AssetConfig.IdTypes, AssetIdsByType);
        EnumsByAssetId = FindEnumsByAssetId(AssetConfig.IdTypes, AssetIdsByType);
        // HandleIsomorphism(AssetConfig.IdTypes);

        // TODO: Build family based on IsomorphicToAttribute.
        // * AssetTypes in a family need to have a single-type AssetId
        // * AssetType families must have a single unambiguous leader
        // * The child types inherit their enum names from the leader
        // * Child types' mod specific enums need to be in 1:1 relationship with CopiedFrom attrib?
        // ....getting complicated.
    }

    static Dictionary<string, string[]> FindEnumsByAssetId(IDictionary<string, AssetTypeInfo> enums, ILookup<AssetType, string> assetIdsByType) =>
        (from e in enums
            from assetId in assetIdsByType[e.Value.AssetType]
            group e.Value.EnumType by assetId into g
            select (g.Key, g.ToArray()))
        .ToDictionary(x => x.Key, x => x.Item2);

    static ILookup<AssetType, string> FindAssetIdsByType(AssetIdConfig config) =>
        (from kvp in config.Mappings
            let assetId = kvp.Key
            from assetType in kvp.Value
            select (assetType, assetId))
        .ToLookup(x => x.assetType, x => x.assetId);

    static Dictionary<string, string[]> FindAssetIdsForEnums(IDictionary<string, AssetTypeInfo> enums, ILookup<AssetType, string> assetIdsByType) =>
        enums.ToDictionary(
            e => e.Key,
            e => assetIdsByType[e.Value.AssetType].ToArray());

    // Get all AssetIds that share a type, then exclude any that aren't a proper superset of this id.
    static Dictionary<string, string[]> FindAssetIdParents(AssetIdConfig idConfig, ILookup<AssetType, string> assetIdsByType) =>
        idConfig.Mappings.ToDictionary(x => x.Key,
            kvp => kvp.Value
                .SelectMany(x => assetIdsByType[x])
                .Distinct()
                .Where(x => IsSuperSet(idConfig.Mappings[x], kvp.Value))
                .ToArray());

    static bool IsSuperSet(IEnumerable<AssetType> a, IEnumerable<AssetType> b) => new HashSet<AssetType>(a).IsProperSupersetOf(b);

    /*
    static Dictionary<string, EnumData> LoadEnums(AssetConfig config)
    {
        var enums = new Dictionary<string, EnumData>();
        foreach (var file in config.Files)
        {
            if (!enums.TryGetValue(typeInfo.Key, out var e))
            {
                e = new EnumData
                {
                    FullName = typeInfo.Key,
                    EnumType = typeInfo.Value.EnumType,
                    AssetType = typeInfo.Value.AssetType,
                    CopiedFrom = typeInfo.Value.CopiedFrom,
                };
                enums[typeInfo.Key] = e;
            }

            foreach (var o in file.Assets.Values.OrderBy(x => x.Id))
            {
                var id = o.Id;

                if (e.EnumType == "byte" && id > 0xff)
                {
                    throw new InvalidOperationException(
                        $"Enum {e.FullName} has an underlying type of byte, but it " +
                        $"defines a value {id} which is greater than the maximum value that a byte can represent (256).");
                }

                e.Entries.Add(string.IsNullOrEmpty(o.Name)
                    ? new EnumEntry {Name = $"Unknown{id}", Value = id}
                    : new EnumEntry {Name = Sanitise(o.Name), Value = id});
            }
        }

        return enums;
    }

    static void DeduplicateEnums(Dictionary<string, EnumData> enums)
    {
        foreach (var e in enums.Values)
        {
            var counters = 
                e.Entries
                .GroupBy(x => x.Name)
                .Where(x => x.Count() > 1)
                .ToDictionary(x => x.Key, x => 1);

            foreach (var o in e.Entries)
            {
                if (!counters.ContainsKey(o.Name))
                    continue;
                var name = o.Name;

                int count = counters[name];
                o.Name = count == 1 ? name : name + count;
                counters[name]++;
            }
        }
    }

    static void HandleIsomorphism(Dictionary<string, EnumData> enums)
    {
        foreach (var e in enums.Values)
        {
            var type = typeof(AssetType);
            var memberInfo = type.GetMember(e.AssetType.ToString()).SingleOrDefault();
            if (memberInfo == null)
                continue;

            if (!(memberInfo.GetCustomAttributes(typeof(IsomorphicToAttribute), false).FirstOrDefault() is IsomorphicToAttribute iso))
                continue;

            if (e.Entries.Any(x => !x.Name.StartsWith("Unknown")))
            {
                throw new InvalidOperationException(
                    $"Enum {e.FullName} identifies assets of type {e.AssetType}, which " +
                    $"is defined as being isomorphic to {iso.Type} causing its entries to be defined by the " +
                    $"enums associated with {iso.Type}, however it declares {e.Entries.Count} entries of its own.");
            }
            e.Entries.Clear();

            if (string.IsNullOrEmpty(e.CopiedFrom))
            {
                throw new InvalidOperationException(
                    $"Enum {e.FullName} identifies assets of type {e.AssetType}, which " +
                    $"is defined as being isomorphic to {iso.Type} causing its entries to be defined by the " +
                    $"enums associated with {iso.Type}, however it does not have a CopiesFrom property identifying the" +
                    "enum type to copy entries from.");
            }

            if(!enums.TryGetValue(e.CopiedFrom, out var parentData))
            {
                throw new InvalidOperationException(
                    $"Enum {e.FullName} specifies the type {e.CopiedFrom} as its CopiedFrom property, but this type is" +
                    " not defined by the current mod's configuration, or that of any dependency.");
            }

            if (parentData.AssetType != iso.Type)
            {
                throw new InvalidOperationException(
                    $"Enum {e.FullName} specifies the type {e.CopiedFrom} as its CopiedFrom property, but its type" +
                    $" is {parentData.AssetType} which does not match the expected asset type declared by the IsomorphicTo attribute ({iso.Type})");
            }

            foreach (var entry in parentData.Entries)
                e.Entries.Add(entry);
        }
    }
    */

    static readonly char[] ForbiddenCharacters = { ' ', '\t', '-', '(', ')', ',', '?', '.', '"' };
    static string Sanitise(string x)
    {
        var chars = new List<char>();
        bool capitaliseNext = true;
        foreach (var c in x)
        {
            if (c == '\'')
                continue;

            if (!ForbiddenCharacters.Contains(c))
            {
                chars.Add(capitaliseNext ? char.ToUpper(c) : c);
                capitaliseNext = false;
            }
            else capitaliseNext = true;
        }

        return new string(chars.ToArray());
    }
}