using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Config
{
    public enum XldObjectType
    {
        Unknown,

        // Graphics
        AmorphousSprite,
        FixedSizeSprite,
        HeaderPerSubImageSprite,
        InterlacedBitmap,
        Palette,
        PaletteCommon,
        SingleHeaderSprite,
        Video,

        // Maps
        MapData,
        IconData,

        // Audio
        AudioSample,
        SampleLibrary,
        Song,

        // Text
        StringTable,
        Script,

        // Misc
        BlockList,
        CharacterData,
        EventSet,
        Inventory,
        MonsterGroup,
        SpellData,
        TranslationTable,
        LabyrinthData,
    }

    public class AssetConfig
    {
        public class Xld
        {
            [JsonIgnore] public string Name;
            public string EnumName;
            [JsonConverter(typeof(StringEnumConverter))]
            public XldObjectType Type;
            public int? Width;
            public int? Height;
            public bool? RotatedLeft;
            public IDictionary<int, Asset> Assets { get; } = new Dictionary<int, Asset>();
        }

        public class Asset
        {
            [JsonIgnore] public Xld Parent;
            [JsonIgnore] public int Id;
            [JsonIgnore] public XldObjectType Type;

            public string Name;
            public int? Width;
            public int? Height;
            public string SubSprites;
            public IList<int> PaletteHints;
            public bool? UseSmallGraphics;
            public string FrameCountOverrides;
            [JsonIgnore] public int EffectiveWidth => Width ?? Parent.Width ?? 0;
            [JsonIgnore] public int EffectiveHeight => Height ?? Parent.Height ?? 0;
        }

        [JsonIgnore]
        public string BasePath { get; set; }

        [JsonIgnore] public string BaseDataPath => Path.Combine(BasePath, "data");
        public string XldPath { get; set; }
        public string ExportedXldPath { get; set; }

        public IDictionary<string, Xld> Xlds { get; } = new Dictionary<string, Xld>();

        public static AssetConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "assets.json");
            AssetConfig config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<AssetConfig>(configText);

                foreach (var xld in config.Xlds)
                {
                    xld.Value.Name = xld.Key;
                    foreach(var o in xld.Value.Assets)
                    {
                        o.Value.Parent = xld.Value;
                        o.Value.Id = o.Key;
                        o.Value.Type = xld.Value.Type;
                        o.Value.PaletteHints = o.Value.PaletteHints ?? new List<int>();
                    }
                }
            }
            else
            {
                config = new AssetConfig
                {
                    XldPath = @"albion_sr\CD\XLDLIBS",
                    ExportedXldPath = @"exported"
                };
            }
            config.BasePath = basePath;
            return config;
        }

        public void Save()
        {
            foreach (var xld in Xlds)
            {
                if (xld.Value.RotatedLeft == false)
                    xld.Value.RotatedLeft = null;
                foreach (var asset in xld.Value.Assets)
                {
                    if (string.IsNullOrWhiteSpace(asset.Value.Name))
                        asset.Value.Name = null;

                    if (asset.Value.PaletteHints != null && !asset.Value.PaletteHints.Any())
                        asset.Value.PaletteHints = null;
                }
            }

            var configPath = Path.Combine(BasePath, "data", "assets.json");
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);

            foreach (var xld in Xlds)
            {
                foreach (var asset in xld.Value.Assets)
                {
                    if (asset.Value.PaletteHints == null)
                        asset.Value.PaletteHints = new List<int>();
                }
            }
        }
    }
}
