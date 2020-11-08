using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;

namespace UAlbion.CodeGenerator
{
    public class Assets
    {
        public string BaseDir { get; }
        public AssetConfig AssetConfig { get; }
        public AssetIdConfig AssetIdConfig { get; }
        public CoreSpriteConfig CoreSpriteConfig { get; }
        public Dictionary<string, EnumData> Enums { get; }
        public Dictionary<string, string[]> ParentsByAssetId { get; }
        public Dictionary<string, string[]> AssetIdsByEnum { get; }
        public Dictionary<string, string[]> EnumsByAssetId { get; set; }
        public ILookup<AssetType, string> AssetIdsByType { get; }
        public Dictionary<AssetType, string[]> FamilyMembersByAssetId { get; }

        public Assets() // Everything in this class should be treated as read-only once the constructor finishes.
        {
            BaseDir = ConfigUtil.FindBasePath();
            var assetIdConfigPath = Path.Combine(BaseDir, @"src/Formats/AssetIdTypes.json");

            AssetConfig = AssetConfig.Load(BaseDir);
            AssetIdConfig = AssetIdConfig.Load(assetIdConfigPath);
            CoreSpriteConfig = CoreSpriteConfig.Load(BaseDir);

            Enums = LoadEnums(AssetConfig);
            AddCoreSprites(Enums, CoreSpriteConfig);
            DeduplicateEnums(Enums);
            AssetIdsByType = FindAssetIdsByType(AssetIdConfig);
            ParentsByAssetId = FindAssetIdParents(AssetIdConfig, AssetIdsByType);
            AssetIdsByEnum = FindAssetIdsForEnums(Enums, AssetIdsByType);
            EnumsByAssetId = FindEnumsByAssetId(Enums, AssetIdsByType);
            HandleIsomorphism(Enums);

            // TODO: Build family based on IsomorphicToAttribute.
            // * AssetTypes in a family need to have a single-type AssetId
            // * AssetType families must have a single unambiguous leader
            // * The child types inherit their enum names from the leader
            // * Child types' mod specific enums need to be in 1:1 relationship with CopiedFrom attrib?
            // ....getting complicated.
        }

        Dictionary<string, string[]> FindEnumsByAssetId(Dictionary<string, EnumData> enums, ILookup<AssetType, string> assetIdsByType) =>
            (from e in enums
             from assetId in assetIdsByType[e.Value.AssetType]
             group e.Key by assetId into g
             select (g.Key, g.ToArray()))
            .ToDictionary(x => x.Key, x => x.Item2);

        static ILookup<AssetType, string> FindAssetIdsByType(AssetIdConfig config) =>
            (from kvp in config.Mappings
                let assetId = kvp.Key
                from assetType in kvp.Value
                select (assetType, assetId))
            .ToLookup(x => x.assetType, x => x.assetId);

        static Dictionary<string, string[]> FindAssetIdsForEnums(Dictionary<string, EnumData> enums, ILookup<AssetType, string> assetIdsByType) =>
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

        static Dictionary<string, EnumData> LoadEnums(AssetConfig config)
        {
            var enums = new Dictionary<string, EnumData>();
            foreach (var typeInfo in config.Types)
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

                foreach (var file in typeInfo.Value.Files.Values)
                {
                    foreach (var o in file.Assets.Values.OrderBy(x => x.Id))
                    {
                        var id = o.Id;

                        if (e.EnumType == "byte" && id > 0xff)
                            continue;

                        e.Entries.Add(string.IsNullOrEmpty(o.Name)
                            ? new EnumEntry {Name = $"Unknown{id}", Value = id}
                            : new EnumEntry {Name = Sanitise(o.Name), Value = id});
                    }
                }
            }

            return enums;
        }

        static void AddCoreSprites(Dictionary<string, EnumData> enums, CoreSpriteConfig config)
        {
            const string coreSprite = "UAlbion.Base.CoreSprite";
            enums[coreSprite] = new EnumData { FullName = coreSprite, EnumType = "byte" };
            foreach (var item in config.CoreSpriteIds)
                enums[coreSprite].Entries.Add(new EnumEntry { Name = Sanitise(item.Value), Value = item.Key });
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
            }
        }

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
}