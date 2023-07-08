using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Containers;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.TestCommon;
using Xunit;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace UAlbion.Config.Tests;

public class AssetConfigTests : Component
{
    static readonly IJsonUtil JsonUtil = new JsonUtil();

    // TODO: Add MapFile and Map tests

    const string TypeConfig1 = @"
{
  ""IdTypes"": {
    ""3dobj"":    { ""AssetType"": ""Object3D"",         ""EnumType"": ""UAlbion.Base.DungeonObject, UAlbion.Base"" },
    ""autotile"": { ""AssetType"": ""AutomapGfx"",       ""EnumType"": ""UAlbion.Base.AutomapTiles, UAlbion.Base"" },
    ""block"":    { ""AssetType"": ""BlockList"",        ""EnumType"": ""UAlbion.Base.BlockList, UAlbion.Base"" },
    ""combg"":    { ""AssetType"": ""CombatBackground"", ""EnumType"": ""UAlbion.Base.CombatBackground, UAlbion.Base"" },
    ""comgfx"":   { ""AssetType"": ""CombatGfx"",        ""EnumType"": ""UAlbion.Base.CombatGfx, UAlbion.Base"" },
    ""coregfx"":  { ""AssetType"": ""CoreGfx"",          ""EnumType"": ""UAlbion.Base.CoreGfx, UAlbion.Base"" },
    ""floor"":    { ""AssetType"": ""Floor"",            ""EnumType"": ""UAlbion.Base.Floor, UAlbion.Base"" },
    ""font"":     { ""AssetType"": ""FontDefinition"",   ""EnumType"": ""UAlbion.Base.Font, UAlbion.Base"" },
    ""fontgfx"":  { ""AssetType"": ""FontGfx"",          ""EnumType"": ""UAlbion.Base.FontGfx, UAlbion.Base"" },
    ""item"":     { ""AssetType"": ""Item"",             ""EnumType"": ""UAlbion.Base.Item, UAlbion.Base"" },
    ""itemname"": { ""AssetType"": ""ItemName"",         ""EnumType"": ""UAlbion.Base.ItemName, UAlbion.Base"" },
    ""overlay"":  { ""AssetType"": ""WallOverlay"",      ""EnumType"": ""UAlbion.Base.WallOverlay, UAlbion.Base"" },
    ""pal"":      { ""AssetType"": ""Palette"",          ""EnumType"": ""UAlbion.Base.Palette, UAlbion.Base"" },
    ""special"":  { ""AssetType"": ""Special"",          ""EnumType"": ""UAlbion.Base.Special, UAlbion.Base"" },
    ""spell"":    { ""AssetType"": ""Spell"",            ""EnumType"": ""UAlbion.Base.Spell, UAlbion.Base"" },
    ""stext"":    { ""AssetType"": ""Text"",             ""EnumType"": ""UAlbion.Base.SystemText, UAlbion.Base"" },
    ""tiledata"": { ""AssetType"": ""Tileset"",          ""EnumType"": ""UAlbion.Base.Tileset, UAlbion.Base"" },
    ""tilegfx"":  { ""AssetType"": ""TilesetGfx"",       ""EnumType"": ""UAlbion.Base.TilesetGfx, UAlbion.Base"" },
    ""utext"":    { ""AssetType"": ""Text"",             ""EnumType"": ""UAlbion.Base.UAlbionString, UAlbion.Base"" },
    ""word"":     { ""AssetType"": ""Word"",             ""EnumType"": ""UAlbion.Base.Word, UAlbion.Base"" }
  },

  ""Loaders"": {
    ""amorphous"":    ""UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats"",
    ""block"":        ""UAlbion.Formats.Parsers.BlockListLoader, UAlbion.Formats"",
    ""fixedsize"":    ""UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats"",
    ""font"":         ""UAlbion.Formats.Parsers.JsonLoader`1[[UAlbion.Formats.Assets.FontDefinition, UAlbion.Formats]], UAlbion.Formats"",
    ""header"":       ""UAlbion.Formats.Parsers.SingleHeaderSpriteLoader, UAlbion.Formats"",
    ""itemdata"":     ""UAlbion.Formats.Parsers.ItemDataLoader, UAlbion.Formats"",
    ""itemname"":     ""UAlbion.Formats.Parsers.ItemNameLoader, UAlbion.Formats"",
    ""itemnameMeta"": ""UAlbion.Formats.Parsers.ItemNameMetaLoader, UAlbion.Formats"",
    ""json"":         ""UAlbion.Formats.Parsers.JsonStringLoader, UAlbion.Formats"",
    ""multiheader"":  ""UAlbion.Formats.Parsers.MultiHeaderSpriteLoader, UAlbion.Formats"",
    ""pal"":          ""UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats"",
    ""soundbank"":    ""UAlbion.Game.Assets.SoundBankLoader, UAlbion.Game"",
    ""spell"":        ""UAlbion.Formats.Parsers.SpellLoader, UAlbion.Formats"",
    ""stext"":        ""UAlbion.Formats.Parsers.SystemTextLoader, UAlbion.Formats"",
    ""stringset"":    ""UAlbion.Formats.Parsers.StringSetStringLoader, UAlbion.Formats"",
    ""tilegfx"":      ""UAlbion.Formats.Parsers.TilesetGraphicsLoader, UAlbion.Formats"",
    ""tileset"":      ""UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats"",
    ""wordlist"":     ""UAlbion.Formats.Parsers.WordListLoader, UAlbion.Formats""
  },

  ""Containers"": {
    ""binaryoffsets"": ""UAlbion.Formats.Containers.BinaryOffsetContainer, UAlbion.Formats"",
    ""items"":         ""UAlbion.Formats.Containers.ItemListContainer, UAlbion.Formats"",
    ""raw"":           ""UAlbion.Formats.Containers.RawContainer, UAlbion.Formats"",
    ""spells"":        ""UAlbion.Formats.Containers.SpellListContainer, UAlbion.Formats""
  },

  ""PostProcessors"": {
    ""atlas"": ""UAlbion.Formats.Parsers.AtlasPostProcessor, UAlbion.Formats""
  },

  ""GlobalPropertyTypes"": [ ""UAlbion.Config.Properties.AssetProps, UAlbion.Config"" ]
}
";

    const string AssetConfig1 = @"
{
  ""special.SoundBank"": { ""Files"": { ""Albion/DRIVERS/ALBISND.OPL"": { ""Container"": ""raw"", ""Loader"": ""soundbank"" } } },
  ""itemname.1-462"": {
    ""Files"": { // Dummy files used for ensuring the language is available on the AssetLoadContext
      ""!GERMAN"": { ""Language"": ""GERMAN"" },
      ""!ENGLISH"": { ""Language"": ""ENGLISH"" },
      ""!FRENCH"": { ""Language"": ""FRENCH"" }
    },
    ""Loader"": ""stringset"",
    ""FirstId"": ""itemname.1"",
    ""Target"": ""special.ItemNamesSingleLang"",
    ""IsReadOnly"": true 
  },

  ""special.ItemNamesSingleLang"": {
    ""Files"": {
      ""!GERMAN"": { ""Language"": ""GERMAN"" },
      ""!ENGLISH"": { ""Language"": ""ENGLISH"" },
      ""!FRENCH"": { ""Language"": ""FRENCH"" }
    },
    ""Loader"": ""itemnameMeta"",
    ""Target"": ""special.ItemNamesMultiLang"",
    ""IsReadOnly"": true 
  },

  ""special.ItemNamesMultiLang"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/ITEMNAME.DAT"": {
        ""Loader"": ""itemname"",
        ""Container"": ""raw""
      }
    }
  },
  ""item.1-462"":        { ""Files"": { ""Albion/CD/XLDLIBS/ITEMLIST.DAT"": { ""Loader"": ""itemdata"", ""Container"": ""items"" } } },

  ""block.1-11"":  { ""Files"": { ""Albion/CD/XLDLIBS/BLKLIST0.XLD"": { ""Loader"": ""block"" } } },
  ""floor.200-299"":    { ""Files"": { ""Albion/CD/XLDLIBS/3DFLOOR2.XLD"": { ""Loader"": ""fixedsize"", ""Width"": 64, ""Height"": 64 } } },
  ""combg.1-19"":       { ""Files"": { ""Albion/CD/XLDLIBS/COMBACK0.XLD"": { ""Loader"": ""fixedsize"", ""Width"": 360 } } },
  ""tilegfx.1-11"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/ICONGFX0.XLD"": {
        ""Loader"": ""tilegfx"",
        ""Width"": 16,
        ""Height"": 16,
        ""Map"": {
          ""tilegfx.1"":  { ""Palette"": ""pal.1"" },
          ""tilegfx.2"":  { ""Palette"": ""pal.2"" },
          ""tilegfx.3"":  { ""Palette"": ""pal.6"" },
          ""tilegfx.4"":  { ""Palette"": ""pal.4"" },
          ""tilegfx.5"":  { ""Palette"": ""pal.5"" },
          ""tilegfx.6"":  { ""Palette"": ""pal.16"" },
          ""tilegfx.7"":  { ""Palette"": ""pal.9"" },
          ""tilegfx.8"":  { ""Palette"": ""pal.26"" },
          ""tilegfx.9"":  { ""Palette"": ""pal.28"" },
          ""tilegfx.10"": { ""Palette"": ""pal.45"" },
          ""tilegfx.11"": { ""Palette"": ""pal.56"" }
        }
      }
    }
  },
  ""comgfx.1-85"":      { ""Files"": { ""Albion/CD/XLDLIBS/COMGFX0.XLD"": {  ""Loader"": ""multiheader"", ""Palette"": ""pal.23"" } } },

  ""utext.0-*"":      { ""Loader"": ""stringset"", ""FirstId"": ""utext.0"",    ""Target"": ""special.UAlbionStrings"" },
  ""stext.0-777"":    { ""Loader"": ""stringset"", ""FirstId"": ""stext.0"",    ""Target"": ""special.SystemStrings"" },

  ""autotile.1-2"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/AUTOGFX0.XLD"": {
        ""Post"": ""atlas"",
        ""Loader"": ""amorphous"",
        ""Map"": {
          ""autotile.1"": { ""SubSprites"": ""(8,8,576) (16,16)"", ""Palette"": ""pal.11"" },
          ""autotile.2"": { ""SubSprites"": ""(8,8,576) (16,16)"", ""Palette"": ""pal.30"" }
        }
      }
    }
  },

  ""3dobj.1-99"":       { ""Files"": { ""Albion/CD/XLDLIBS/3DOBJEC0.XLD"": { ""Loader"": ""fixedsize"" } } },
  ""overlay.1-99"":     { ""Files"": { ""Albion/CD/XLDLIBS/3DOVERL0.XLD"": { ""Loader"": ""fixedsize"", ""Transposed"": true } } },
  ""fontgfx.1-2"": { // English/French fonts, regular & bold
    ""Files"": {
      ""Albion/CD/XLDLIBS/FONTS0.XLD#33906F62"": { // EN+FR
        ""Loader"": ""fixedsize"",
        ""Optional"": true,
        ""Width"": 8,
        ""Height"": 8
      }
    }
  },

  ""tiledata.1-11"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/ICONDAT0.XLD"": {
        ""Loader"": ""tileset"",
        ""Map"": {
          ""tiledata.1"": { ""UseSmallGraphics"": true },
          ""tiledata.2"": { ""UseSmallGraphics"": true },
          ""tiledata.4"": { ""UseSmallGraphics"": true }
        }
      }
    }
  },

  ""pal.0"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/PALETTE.000"": {
        ""IsCommon"": true,
        ""Container"": ""raw"",
        ""Loader"": ""pal"" 
      }
    }
  },

  ""pal.1-56"": {
    ""Files"": {
      ""Albion/CD/XLDLIBS/PALETTE0.XLD"": {
        ""Loader"": ""pal"",
        ""Map"": {
          ""pal.1"":  { ""NightPalette"": ""pal.47"", ""AnimatedRanges"": ""0x99-0x9f, 0xb0-0xb4, 0xb5-0xbf"" },
          ""pal.2"":  { ""NightPalette"": ""pal.47"", ""AnimatedRanges"": ""0x99-0x9f, 0xb0-0xb4, 0xb5-0xbf"" },
          ""pal.3"":  { ""NightPalette"": ""pal.55"",  ""AnimatedRanges"": ""0x40-0x43, 0x44-0x4f"" },
          ""pal.4"":  { ""NightPalette"": ""pal.48"" },
          ""pal.6"":  { ""AnimatedRanges"": ""0xb0-0xb4, 0xb5-0xbf"" },
          ""pal.14"": { ""NightPalette"": ""pal.49"", ""AnimatedRanges"": ""0xb0-0xb3, 0xb4-0xbf"" },
          ""pal.15"": { ""AnimatedRanges"": ""0x58-0x5f"" },
          ""pal.25"": { ""NightPalette"": ""pal.49"", ""AnimatedRanges"": ""0xb0-0xb3, 0xb4-0xbf"" },
          ""pal.26"": { ""AnimatedRanges"": ""0xb4-0xb7, 0xb8-0xbb, 0xbc-0xbf"" },
          ""pal.31"": { ""AnimatedRanges"": ""0x10-0x4f"" },
          ""pal.47"": { ""AnimatedRanges"": ""0x99-0x9f, 0xb0-0xb4, 0xb5-0xbf"" },
          ""pal.49"": { ""AnimatedRanges"": ""0xb0-0xb3, 0xb4-0xbf"" },
          ""pal.51"": { ""NightPalette"": ""pal.49"", ""AnimatedRanges"": ""0xb0-0xb3, 0xb4-0xbf"" },
          ""pal.55"": { ""AnimatedRanges"": ""0x40-0x43, 0x44-0x4f"" }
        }
      }
    }
  },

  ""spell.1-210"": { ""Files"": { ""Albion/CD/XLDLIBS/SPELLDAT.DAT"": { ""Loader"": ""spell"", ""Container"": ""spells"" } } },
  ""coregfx.0-88"": {
    ""Files"": {
      ""Albion/MAIN.EXE#476227B0"": {}, // EN GOG, built Aug 22 1996
      ""Albion/MAIN.EXE#9FC7ABCF"": {}, // EN, built Jul 25 1996
      ""Albion/MAIN.EXE#487DA334"": {}, // FR
      ""Albion/MAIN.EXE#EC6D6389"": {} // DE GOG ISO, built Dec 14 1995
    },
    ""Container"": ""binaryoffsets"",
    ""Loader"": ""fixedsize"",
    ""IsReadOnly"": true
  }
}
";

    TypeConfig TypeConfig { get; }
    AssetConfig AssetConfig { get; }

    public AssetConfigTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;

        var tcl = new TypeConfigLoader(JsonUtil);
        var typeConfigBytes = Encoding.UTF8.GetBytes(TypeConfig1);
        TypeConfig = tcl.Parse(typeConfigBytes, "Test", null, AssetMapping.Global);

        foreach (var kvp in TypeConfig.IdTypes)
            AssetMapping.Global.RegisterAssetType(Type.GetType(kvp.Value.EnumType), kvp.Value.AssetType);

        var disk = new MockFileSystem(false);
        var baseDir = @"C:\ualbion";
        var pathResolver = new PathResolver(baseDir, nameof(AssetConfigTests));
        var acl = new AssetConfigLoader(disk, JsonUtil, pathResolver, TypeConfig);
        var assetConfigBytes = Encoding.UTF8.GetBytes(AssetConfig1);
        AssetConfig = acl.Parse(assetConfigBytes, nameof(AssetConfigTests), null);
    }

    [Fact]
    public void VerifyIdTypes()
    {
        Assert.Collection(TypeConfig.IdTypes.Values.OrderBy(x => x.Alias),
            t =>
            {
                Assert.Equal("3dobj", t.Alias);
                Assert.Equal(AssetType.Object3D, t.AssetType);
                Assert.Equal("UAlbion.Base.DungeonObject, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("autotile", t.Alias);
                Assert.Equal(AssetType.AutomapGfx, t.AssetType);
                Assert.Equal("UAlbion.Base.AutomapTiles, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("block", t.Alias);
                Assert.Equal(AssetType.BlockList, t.AssetType);
                Assert.Equal("UAlbion.Base.BlockList, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("combg", t.Alias);
                Assert.Equal(AssetType.CombatBackground, t.AssetType);
                Assert.Equal("UAlbion.Base.CombatBackground, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("comgfx", t.Alias);
                Assert.Equal(AssetType.CombatGfx, t.AssetType);
                Assert.Equal("UAlbion.Base.CombatGfx, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("coregfx", t.Alias);
                Assert.Equal(AssetType.CoreGfx, t.AssetType);
                Assert.Equal("UAlbion.Base.CoreGfx, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("floor", t.Alias);
                Assert.Equal(AssetType.Floor, t.AssetType);
                Assert.Equal("UAlbion.Base.Floor, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("font", t.Alias);
                Assert.Equal(AssetType.FontDefinition, t.AssetType);
                Assert.Equal("UAlbion.Base.Font, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("fontgfx", t.Alias);
                Assert.Equal(AssetType.FontGfx, t.AssetType);
                Assert.Equal("UAlbion.Base.FontGfx, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("item", t.Alias);
                Assert.Equal(AssetType.Item, t.AssetType);
                Assert.Equal("UAlbion.Base.Item, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("itemname", t.Alias);
                Assert.Equal(AssetType.ItemName, t.AssetType);
                Assert.Equal("UAlbion.Base.ItemName, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("overlay", t.Alias);
                Assert.Equal(AssetType.WallOverlay, t.AssetType);
                Assert.Equal("UAlbion.Base.WallOverlay, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("pal", t.Alias);
                Assert.Equal(AssetType.Palette, t.AssetType);
                Assert.Equal("UAlbion.Base.Palette, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("special", t.Alias);
                Assert.Equal(AssetType.Special, t.AssetType);
                Assert.Equal("UAlbion.Base.Special, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("spell", t.Alias);
                Assert.Equal(AssetType.Spell, t.AssetType);
                Assert.Equal("UAlbion.Base.Spell, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("stext", t.Alias);
                Assert.Equal(AssetType.Text, t.AssetType);
                Assert.Equal("UAlbion.Base.SystemText, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("tiledata", t.Alias);
                Assert.Equal(AssetType.Tileset, t.AssetType);
                Assert.Equal("UAlbion.Base.Tileset, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("tilegfx", t.Alias);
                Assert.Equal(AssetType.TilesetGfx, t.AssetType);
                Assert.Equal("UAlbion.Base.TilesetGfx, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("utext", t.Alias);
                Assert.Equal(AssetType.Text, t.AssetType);
                Assert.Equal("UAlbion.Base.UAlbionString, UAlbion.Base", t.EnumType);
            },
            t =>
            {
                Assert.Equal("word", t.Alias);
                Assert.Equal(AssetType.Word, t.AssetType);
                Assert.Equal("UAlbion.Base.Word, UAlbion.Base", t.EnumType);
            });
    }

    [Fact]
    public void VerifyLoaders()
    {
        Assert.Collection(TypeConfig.Loaders.OrderBy(x => x.Key),
            l =>
            {
                Assert.Equal("amorphous", l.Key);
                Assert.Equal(typeof(AmorphousSpriteLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("block", l.Key);
                Assert.Equal(typeof(BlockListLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("fixedsize", l.Key);
                Assert.Equal(typeof(FixedSizeSpriteLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("font", l.Key);
                Assert.Equal(typeof(JsonLoader<FontDefinition>), l.Value);
            },
            l =>
            {
                Assert.Equal("header", l.Key);
                Assert.Equal(typeof(SingleHeaderSpriteLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("itemdata", l.Key);
                Assert.Equal(typeof(ItemDataLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("itemname", l.Key);
                Assert.Equal(typeof(ItemNameLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("itemnameMeta", l.Key);
                Assert.Equal(typeof(ItemNameMetaLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("json", l.Key);
                Assert.Equal(typeof(JsonStringLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("multiheader", l.Key);
                Assert.Equal(typeof(MultiHeaderSpriteLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("pal", l.Key);
                Assert.Equal(typeof(PaletteLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("soundbank", l.Key);
                Assert.Equal(typeof(SoundBankLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("spell", l.Key);
                Assert.Equal(typeof(SpellLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("stext", l.Key);
                Assert.Equal(typeof(SystemTextLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("stringset", l.Key);
                Assert.Equal(typeof(StringSetStringLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("tilegfx", l.Key);
                Assert.Equal(typeof(TilesetGraphicsLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("tileset", l.Key);
                Assert.Equal(typeof(TilesetLoader), l.Value);
            },
            l =>
            {
                Assert.Equal("wordlist", l.Key);
                Assert.Equal(typeof(WordListLoader), l.Value);
            });
    }
#if false
    [Fact]
    public void VerifyRanges()
    {
        var ordered = AssetConfig.Ranges.AllRanges.OrderBy(x => x.Range.From);
        Assert.Collection(ordered.Select(x => x.Range),
            x => Assert.Equal(new AssetRange(AssetId.None, AssetId.None), x),
            x => Assert.Equal(new AssetRange(AssetId.None, AssetId.None), x)
        );
    }

    [Fact]
    public void VerifySpecial()
    {
        var f = TypeConfig.Files["$(ALBION)/DRIVERS/ALBISND.OPL"];
        Assert.Equal("UAlbion.Game.Assets.SoundBankLoader, UAlbion.Game", f.Loader);
        Assert.Null(f.Container);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("special.SoundBank", m.Value.Id);
            });
    }

    [Fact]
    public void VerifyItemList()
    {
        var f = TypeConfig.Files["$(XLD)/ITEMLIST.DAT"];
        Assert.Equal("UAlbion.Formats.Parsers.ItemDataLoader, UAlbion.Formats", f.Loader);
        Assert.Equal("UAlbion.Formats.Containers.ItemListContainer, UAlbion.Formats", f.Container);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("item.1", m.Value.Id);
            });
    }

    [Fact]
    public void VerifyBlockList()
    {
        var f = TypeConfig.Files["$(XLD)/BLKLIST0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.BlockListLoader, UAlbion.Formats", f.Loader);
        Assert.Null(f.Container);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("block.1", m.Value.Id);
            });
    }
    [Fact]
    public void VerifyFloors()
    {
        var f = TypeConfig.Files["$(XLD)/3DFLOOR2.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Equal(64, f.Width);
        Assert.Equal(64, f.Height);
        Assert.Collection(f.Map.OrderBy(x => x.Key), m =>
        {
            Assert.Equal(0, m.Key);
            Assert.Equal("floor.200", m.Value.Id);
        });
    }

    [Fact]
    public void VerifyCombatBackgrounds()
    {
        var f = TypeConfig.Files["$(XLD)/COMBACK0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Equal(360, f.Width);
        Assert.Collection(f.Map.OrderBy(x => x.Key), m =>
        {
            Assert.Equal(0, m.Key);
            Assert.Equal("combg.1", m.Value.Id);
        });
    }

    [Fact]
    public void VerifyTileGraphics()
    {
        var f = TypeConfig.Files["$(XLD)/ICONGFX0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Equal(16, f.Width);
        Assert.Equal(16, f.Height);
        Assert.Collection(f.Map.OrderBy(x => x.Key), m =>
        {
            Assert.Equal(0, m.Key);
            Assert.Equal("tilegfx.1", m.Value.Id);
        });
    }

    [Fact]
    public void VerifyCombatGraphics()
    {
        var f = TypeConfig.Files["$(XLD)/COMGFX0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.MultiHeaderSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key), m =>
        {
            Assert.Equal(0, m.Key);
            Assert.Equal("comgfx.1", m.Value.Id);
        });
    }

    [Fact]
    public void VerifyAutomapGraphics()
    {
        var f = TypeConfig.Files["$(XLD)/AUTOGFX0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("autotile.1", m.Value.Id);
                Assert.Equal("(8,8,576) (16,16)", m.Value.GetProperty<string>(AssetProps.SubSprites, null));
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.Equal("(8,8,576) (16,16)", m.Value.GetProperty<string>(AssetProps.SubSprites, null));
            });
    }

    [Fact]
    public void Verify3dObjects()
    {
        var f = TypeConfig.Files["$(XLD)/3DOBJEC0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("3dobj.1", m.Value.Id);
                Assert.Equal(32, m.Value.Width);
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.Equal(16, m.Value.Width);
            },
            m =>
            {
                Assert.Equal(26, m.Key);
                Assert.Equal(50, m.Value.Width);
                Assert.Equal(128, m.Value.Height);
            });
    }

    [Fact]
    public void VerifyOverlays()
    {
        var f = TypeConfig.Files["$(XLD)/3DOVERL0.XLD"];
        Assert.True(f.GetProperty(AssetProps.Transposed, false));
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("overlay.1", m.Value.Id);
                Assert.Equal(51, m.Value.Width);
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.Equal(44, m.Value.Width);
            },
            m =>
            {
                Assert.Equal(22, m.Key);
                Assert.Equal(62, m.Value.Width);
                Assert.Equal(42, m.Value.Height);
            });
    }

    [Fact]
    public void VerifyFonts()
    {
        var f = TypeConfig.Files["$(XLD)/FONTS0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Equal(8, f.Width);
        Assert.Equal(8, f.Height);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("font.1", m.Value.Id);
            });
    }

    [Fact]
    public void VerifyTilesets()
    {
        var f = TypeConfig.Files["$(XLD)/ICONDAT0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("tiledata.1", m.Value.Id);
                Assert.True(m.Value.GetProperty(AssetProps.UseSmallGraphics, false));
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.True(m.Value.GetProperty(AssetProps.UseSmallGraphics, false));
            },
            m =>
            {
                Assert.Equal(3, m.Key);
                Assert.True(m.Value.GetProperty(AssetProps.UseSmallGraphics, false));
            });
    }

    [Fact]
    public void VerifyPalettes()
    {
        var f = TypeConfig.Files["$(XLD)/PALETTE0.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats", f.Loader);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("pal.1", m.Value.Id);
                Assert.Equal("0x99-0x9f, 0xb0-0xbf", m.Value.GetProperty(AssetProps.AnimatedRanges, ""));
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.Equal("0x99-0x9f, 0xb0-0xb4, 0xb5-0xbf", m.Value.GetProperty(AssetProps.AnimatedRanges, ""));
            });
    }

    [Fact]
    public void VerifySpells()
    {
        var f = TypeConfig.Files["$(XLD)/SPELLDAT.XLD"];
        Assert.Equal("UAlbion.Formats.Parsers.SpellLoader, UAlbion.Formats", f.Loader);
        Assert.Equal("UAlbion.Formats.Containers.SpellListContainer, UAlbion.Formats", f.Container);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m => { Assert.Equal(0, m.Key); Assert.Equal("spell.1", m.Value.Id); },
            m => { Assert.Equal(30, m.Key); Assert.Equal("spell.257", m.Value.Id); },
            m => { Assert.Equal(60, m.Key); Assert.Equal("spell.513", m.Value.Id); });
    }

/* TODO
        [Fact]
        public void VerifyWords()
        {
            Assert.Equal("UAlbion.Formats.Parsers.WordListLoader, UAlbion.Formats", f.Loader);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                }
            );
        }
*/

    [Fact]
    public void VerifyMain()
    {
        var f = TypeConfig.Files["$(ALBION)/MAIN.EXE#476227b0391cf3452166b7a1d52b012ccf6c86bc9e46886dafbed343e9140710"];
        Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
        Assert.Equal("UAlbion.Formats.Containers.BinaryOffsetContainer, UAlbion.Formats", f.Container);
        Assert.Collection(f.Map.OrderBy(x => x.Key),
            m =>
            {
                Assert.Equal(0, m.Key);
                Assert.Equal("coresprite.0", m.Value.Id);
                Assert.Equal(1031768, m.Value.GetProperty(BinaryOffsetContainer.Offset, 0));
                Assert.Equal(14, m.Value.Width);
                Assert.Equal(14, m.Value.Height);
                Assert.Equal("-6 0", m.Value.GetProperty(BinaryOffsetContainer.Hotspot));
            },
            m =>
            {
                Assert.Equal(1, m.Key);
                Assert.Equal(1031964, m.Value.GetProperty(AssetProperty.Offset, 0));
                Assert.Equal(16, m.Value.Width);
                Assert.Equal(16, m.Value.Height);
                Assert.Equal("0 4", m.Value.GetProperty(BinaryOffsetContainer.Hotspot));
            },
            m =>
            {
                Assert.Equal(27, m.Key);
                Assert.Equal(1039632, m.Value.GetProperty(AssetProperty.Offset, 0));
                Assert.Equal(32, m.Value.Width);
                Assert.Equal(64, m.Value.Height);
                Assert.Null(m.Value.GetProperty<string>("Hotspot", null));
            });
    }
#endif
}