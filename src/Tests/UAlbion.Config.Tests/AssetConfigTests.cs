using System.ComponentModel;
using System.Linq;
using Xunit;

namespace UAlbion.Config.Tests
{
    public class AssetConfigTests : Component
    {
        const string TestConfig1 = @"{
  ""IdTypes"": {
    ""3dobj"":    { ""AssetType"": ""Object3D"",        ""EnumType"": ""UAlbion.Base.DungeonObject, UAlbion.Base"" },
    ""autotile"": { ""AssetType"": ""AutomapGraphics"", ""EnumType"": ""UAlbion.Base.AutomapTiles, UAlbion.Base"" },
    ""block"": {
      ""AssetType"": ""BlockList"",
      ""EnumType"": ""UAlbion.Base.BlockList, UAlbion.Base"",
      ""CopiedFrom"": ""UAlbion.Base.TilesetData, UAlbion.Base""
    },
    ""combg"":      { ""AssetType"": ""CombatBackground"", ""EnumType"": ""UAlbion.Base.CombatBackground, UAlbion.Base"" },
    ""comgfx"":     { ""AssetType"": ""CombatGraphics"",   ""EnumType"": ""UAlbion.Base.CombatGraphics, UAlbion.Base"" },
    ""coresprite"": { ""AssetType"": ""CoreGraphics"",     ""EnumType"": ""UAlbion.Base.CoreSprite, UAlbion.Base"" },
    ""floor"":      { ""AssetType"": ""Floor"",            ""EnumType"": ""UAlbion.Base.Floor, UAlbion.Base"" },
    ""font"":       { ""AssetType"": ""Font"",             ""EnumType"": ""UAlbion.Base.Font, UAlbion.Base"" },
    ""item"":       { ""AssetType"": ""Item"",             ""EnumType"": ""UAlbion.Base.Item, UAlbion.Base"" },
    ""overlay"":    { ""AssetType"": ""WallOverlay"",      ""EnumType"": ""UAlbion.Base.WallOverlay, UAlbion.Base"" },
    ""pal"":        { ""AssetType"": ""Palette"",          ""EnumType"": ""UAlbion.Base.Palette, UAlbion.Base"" },
    ""special"":    { ""AssetType"": ""Special"",          ""EnumType"": ""UAlbion.Base.Special, UAlbion.Base"" },
    ""spell"":      { ""AssetType"": ""Spell"",            ""EnumType"": ""UAlbion.Base.Spell, UAlbion.Base"" },
    ""tiledata"":   { ""AssetType"": ""TilesetData"",      ""EnumType"": ""UAlbion.Base.TilesetData, UAlbion.Base"" },
    ""tilegfx"": {
      ""AssetType"": ""TilesetGraphics"",
      ""EnumType"": ""UAlbion.Base.TilesetGraphics, UAlbion.Base"",
      ""CopiedFrom"": ""UAlbion.Base.TilesetData, UAlbion.Base""
    },
     ""word"": { ""AssetType"": ""Word"", ""EnumType"": ""UAlbion.Base.Word, UAlbion.Base"" }
  },

  ""Loaders"": {
    ""amorphous"":  ""UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats"",
    ""block"":      ""UAlbion.Formats.Parsers.BlockListLoader, UAlbion.Formats"",
    ""fixedsize"":  ""UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats"",
    ""font"":       ""UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats"",
    ""header"":     ""UAlbion.Formats.Parsers.HeaderBasedSpriteLoader, UAlbion.Formats"",
    ""multiheader"": ""UAlbion.Formats.Parsers.MultiHeaderSpriteLoader, UAlbion.Formats"",
    ""itemdata"":   ""UAlbion.Formats.Parsers.ItemDataLoader, UAlbion.Formats"",
    ""json"":       ""UAlbion.Formats.Parsers.JsonStringLoader, UAlbion.Formats"",
    ""pal"":        ""UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats"",
    ""soundbank"":  ""UAlbion.Game.Assets.SoundBankLoader, UAlbion.Game"",
    ""spell"":      ""UAlbion.Formats.Parsers.SpellLoader, UAlbion.Formats"",
    ""systemtext"": ""UAlbion.Formats.Parsers.SystemTextLoader, UAlbion.Formats"",
    ""tileset"":    ""UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats"",
    ""wordlist"":   ""UAlbion.Formats.Parsers.WordListLoader, UAlbion.Formats""
  },

  ""Containers"": {
    ""raw"": ""UAlbion.Formats.Containers.RawContainer, UAlbion.Formats"",
    ""items"": ""UAlbion.Formats.Containers.ItemListContainer, UAlbion.Formats"",
    ""spells"": ""UAlbion.Formats.Containers.SpellListContainer, UAlbion.Formats"",
    ""binaryoffsets"": ""UAlbion.Formats.Containers.BinaryOffsetContainer, UAlbion.Formats""
  },

  ""Files"": {
    ""$(ALBION)/DRIVERS/ALBISND.OPL"": { ""Loader"": ""soundbank"", ""Map"": { ""0"": { ""Id"": ""special.SoundBank"" } } },
    ""$(XLD)/ITEMLIST.DAT"": { ""Loader"": ""itemdata"", ""Container"": ""items"", ""Map"": { ""0"": { ""Id"": ""item.1"" } } },
    ""$(XLD)/BLKLIST0.XLD"": { ""Loader"": ""block"", ""Map"": { ""0"": {""Id"": ""block.1"" } } },
    ""$(XLD)/3DFLOOR2.XLD"": { ""Loader"": ""fixedsize"", ""Width"": 64, ""Height"": 64, ""Map"": { ""0"": { ""Id"": ""floor.200"" } } },
    ""$(XLD)/COMBACK0.XLD"": { ""Loader"": ""fixedsize"", ""Width"": 360, ""Map"": { ""0"": { ""Id"": ""combg.1"" } } },
    ""$(XLD)/ICONGFX0.XLD"": { ""Loader"": ""fixedsize"", ""Width"": 16, ""Height"": 16, ""Map"": { ""0"": { ""Id"": ""tilegfx.1"" } } },
    ""$(XLD)/COMGFX0.XLD"":  { ""Loader"": ""multiheader"", ""Map"": { ""0"": { ""Id"": ""comgfx.1""} } },

    ""$(MOD)/$(LANG)/strings.json"": {
      ""Loader"": ""json"",
      ""Container"": ""raw"",
      ""Map"": { ""0"": { ""Id"": ""special.UAlbionStrings"" } }
    },
    ""$(XLD)/$(LANG)/SYSTEXTS"":     {
      ""Loader"": ""stext"",
      ""Container"": ""raw"",
      ""Map"": { ""0"": { ""Id"": ""special.SystemStrings"" } }
    },
    ""$(XLD)/$(LANG)/WORDLIS0.XLD"": {
      ""Loader"": ""wordlist"",
      ""Map"": {
        ""0"": { ""Id"": ""special.Words1"" },
        ""1"": { ""Id"": ""special.Words2"" },
        ""2"": { ""Id"": ""special.Words3"" }
      }
    },

    ""$(XLD)/AUTOGFX0.XLD"": {
      ""Loader"": ""amorphous"",
      ""Map"": {
        ""0"": { ""Id"": ""autotile.1"", ""SubSprites"": ""(8,8,576) (16,16)"" },
        ""1"": { ""SubSprites"": ""(8,8,576) (16,16)"" }
      }
    },

    ""$(XLD)/3DOBJEC0.XLD"": {
      ""Loader"": ""fixedsize"",
      ""Map"": {
        ""0"": { ""Id"": ""3dobj.1"", ""Width"": 32 },
        ""1"": { ""Width"": 16 },
        ""26"": { ""Width"": 50, ""Height"": 128 }
      }
    },

    ""$(XLD)/3DOVERL0.XLD"": {
      ""Transposed"": true,
      ""Loader"": ""fixedsize"",
      ""Map"": {
        ""0"": { ""Id"": ""overlay.1"", ""Width"": 51 },
        ""1"": { ""Width"": 44 },
        ""22"": { ""Width"": 62, ""Height"": 42 }
      }
    },

    ""$(XLD)/FONTS0.XLD"": {
      ""Loader"": ""font"",
      ""Width"": 8,
      ""Height"": 8,
      ""Map"": {
        ""0"": { ""Id"": ""font.1"", ""Mapping"": ""abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äÄöÖüÜß.:,;'$\""?!/()#%*&+-=><☺♂♀éâàçêëèïîìôòûùáíóú"" },
        ""1"": { ""Mapping"": ""abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äÄöÖüÜß.:,;'$\""?!/()#%*&+-=><☺♂♀éâàçêëèïîìôòûùáíóú"" }
      }
    },

    ""$(XLD)/ICONDAT0.XLD"": {
      ""Loader"": ""tileset"",
      ""Map"": {
        ""0"": { ""Id"": ""tiledata.1"", ""UseSmallGraphics"": true },
        ""1"": { ""UseSmallGraphics"": true },
        ""3"": { ""UseSmallGraphics"": true }
      }
    },

    ""$(XLD)/PALETTE0.XLD"": {
      ""Loader"": ""pal"",
      ""Map"": {
        ""0"": { ""Id"": ""pal.1"", ""AnimatedRanges"": [ ""0x99-0x9f"", ""0xb0-0xbf"" ] },
        ""1"": { ""AnimatedRanges"": [ ""0x99-0x9f"", ""0xb0-0xb4"", ""0xb5-0xbf"" ] }
      }
    },

    ""$(XLD)/SPELLDAT.XLD"": {
      ""Loader"": ""spell"",
      ""Container"": ""spells"",
      ""Map"": { // Ids = 1 + OffsetInSchool + School * 256
        ""0"": { ""Id"": ""spell.1"" },
        ""30"": { ""Id"": ""spell.257"" },
        ""60"": { ""Id"": ""spell.513"" }
      }
    },

    ""$(ALBION)/MAIN.EXE#476227b0391cf3452166b7a1d52b012ccf6c86bc9e46886dafbed343e9140710"": { // EN+DE
      ""Loader"": ""fixedsize"",
      ""Container"": ""binaryoffsets"",
      ""Map"": {
        ""0"": { ""Id"": ""coresprite.0"", ""Offset"": 0x0FBE58, ""Width"": 14, ""Height"": 14, ""Hotspot"": { ""X"": -6,   ""Y"":  0 } },
        ""1"": { ""Offset"": 0x0FBF1C, ""Width"": 16, ""Height"": 16, ""Hotspot"": { ""X"":  0,   ""Y"":  4 } },
        ""27"": { ""Offset"": 0x0FDD10, ""Width"": 32, ""Height"": 64 }
      }
    }
  }
}
";
        [Fact]
        public void VerifyIdTypes()
        {
            var c = AssetConfig.Parse(TestConfig1);
            Assert.Collection(c.IdTypes.Values.OrderBy(x => x.Alias),
                t =>
                {
                    Assert.Equal("3dobj", t.Alias);
                    Assert.Equal(AssetType.Object3D, t.AssetType);
                    Assert.Equal("UAlbion.Base.DungeonObject, UAlbion.Base", t.EnumType);
                },
                t =>
                {
                    Assert.Equal("autotile", t.Alias);
                    Assert.Equal(AssetType.AutomapGraphics, t.AssetType);
                    Assert.Equal("UAlbion.Base.AutomapTiles, UAlbion.Base", t.EnumType);
                },
                t =>
                {
                    Assert.Equal("block", t.Alias);
                    Assert.Equal(AssetType.BlockList, t.AssetType);
                    Assert.Equal("UAlbion.Base.BlockList, UAlbion.Base", t.EnumType);
                    Assert.Equal("UAlbion.Base.TilesetData, UAlbion.Base", t.CopiedFrom);
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
                    Assert.Equal(AssetType.CombatGraphics, t.AssetType);
                    Assert.Equal("UAlbion.Base.CombatGraphics, UAlbion.Base", t.EnumType);
                },
                t =>
                {
                    Assert.Equal("coresprite", t.Alias);
                    Assert.Equal(AssetType.CoreGraphics, t.AssetType);
                    Assert.Equal("UAlbion.Base.CoreSprite, UAlbion.Base", t.EnumType);
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
                    Assert.Equal(AssetType.Font, t.AssetType);
                    Assert.Equal("UAlbion.Base.Font, UAlbion.Base", t.EnumType);
                },
                t =>
                {
                    Assert.Equal("item", t.Alias);
                    Assert.Equal(AssetType.Item, t.AssetType);
                    Assert.Equal("UAlbion.Base.Item, UAlbion.Base", t.EnumType);
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
                    Assert.Equal("tiledata", t.Alias);
                    Assert.Equal(AssetType.TilesetData, t.AssetType);
                    Assert.Equal("UAlbion.Base.TilesetData, UAlbion.Base", t.EnumType);
                },
                t =>
                {
                    Assert.Equal("tilegfx", t.Alias);
                    Assert.Equal(AssetType.TilesetGraphics, t.AssetType);
                    Assert.Equal("UAlbion.Base.TilesetGraphics, UAlbion.Base", t.EnumType);
                    Assert.Equal("UAlbion.Base.TilesetData, UAlbion.Base", t.CopiedFrom);
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
            var c = AssetConfig.Parse(TestConfig1);
            Assert.Collection(c.Loaders.OrderBy(x => x.Key),
                l =>
                {
                    Assert.Equal("amorphous", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("block", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.BlockListLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("fixedsize", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("font", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("header", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.HeaderBasedSpriteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("itemdata", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.ItemDataLoader, UAlbion.Formats", l.Value);
                },
                l =>
                    {
                        Assert.Equal("json", l.Key);
                        Assert.Equal("UAlbion.Formats.Parsers.JsonStringLoader, UAlbion.Formats", l.Value);
                    },
                l =>
                {
                    Assert.Equal("multiheader", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.MultiHeaderSpriteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("pal", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("soundbank", l.Key);
                    Assert.Equal("UAlbion.Game.Assets.SoundBankLoader, UAlbion.Game", l.Value);
                },
                l =>
                {
                    Assert.Equal("spell", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.SpellLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("systemtext", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.SystemTextLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("tileset", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats", l.Value);
                },
                l =>
                {
                    Assert.Equal("wordlist", l.Key);
                    Assert.Equal("UAlbion.Formats.Parsers.WordListLoader, UAlbion.Formats", l.Value);
                });
        }

        [Fact]
        public void VerifyFiles()
        {
            var c = AssetConfig.Parse(TestConfig1);
            Assert.Collection(c.Files.Keys.OrderBy(x => x),
                x => Assert.Equal("$(ALBION)/DRIVERS/ALBISND.OPL", x),
                x => Assert.Equal("$(ALBION)/MAIN.EXE#476227b0391cf3452166b7a1d52b012ccf6c86bc9e46886dafbed343e9140710", x),
                x => Assert.Equal("$(MOD)/$(LANG)/strings.json", x),
                x => Assert.Equal("$(XLD)/$(LANG)/SYSTEXTS", x),
                x => Assert.Equal("$(XLD)/$(LANG)/WORDLIS0.XLD", x),
                x => Assert.Equal("$(XLD)/3DFLOOR2.XLD", x),
                x => Assert.Equal("$(XLD)/3DOBJEC0.XLD", x),
                x => Assert.Equal("$(XLD)/3DOVERL0.XLD", x),
                x => Assert.Equal("$(XLD)/AUTOGFX0.XLD", x),
                x => Assert.Equal("$(XLD)/BLKLIST0.XLD", x),
                x => Assert.Equal("$(XLD)/COMBACK0.XLD", x),
                x => Assert.Equal("$(XLD)/COMGFX0.XLD", x),
                x => Assert.Equal("$(XLD)/FONTS0.XLD", x),
                x => Assert.Equal("$(XLD)/ICONDAT0.XLD", x),
                x => Assert.Equal("$(XLD)/ICONGFX0.XLD", x),
                x => Assert.Equal("$(XLD)/ITEMLIST.DAT", x),
                x => Assert.Equal("$(XLD)/PALETTE0.XLD", x),
                x => Assert.Equal("$(XLD)/SPELLDAT.XLD", x)
            );
        }

        [Fact]
        public void VerifySpecial()
        {
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(ALBION)/DRIVERS/ALBISND.OPL"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/ITEMLIST.DAT"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/BLKLIST0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/3DFLOOR2.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/COMBACK0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/ICONGFX0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/COMGFX0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/AUTOGFX0.XLD"];
            Assert.Equal("UAlbion.Formats.Parsers.AmorphousSpriteLoader, UAlbion.Formats", f.Loader);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                    Assert.Equal("autotile.1", m.Value.Id);
                    Assert.Equal("(8,8,576) (16,16)", m.Value.Get<string>("SubSprites", null));
                },
                m =>
                {
                    Assert.Equal(1, m.Key);
                    Assert.Equal("(8,8,576) (16,16)", m.Value.Get<string>("SubSprites", null));
                });
        }

        [Fact]
        public void Verify3dObjects()
        {
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/3DOBJEC0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/3DOVERL0.XLD"];
            Assert.Equal(true, f.Transposed);
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/FONTS0.XLD"];
            Assert.Equal("UAlbion.Formats.Parsers.FontSpriteLoader, UAlbion.Formats", f.Loader);
            Assert.Equal(8, f.Width);
            Assert.Equal(8, f.Height);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                    Assert.Equal("font.1", m.Value.Id);
                    Assert.Equal(
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äÄöÖüÜß.:,;'$\"?!/()#%*&+-=><☺♂♀éâàçêëèïîìôòûùáíóú",
                        m.Value.Get<string>("Mapping", null));
                },
                m =>
                {
                    Assert.Equal(1, m.Key);
                    Assert.Equal(
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äÄöÖüÜß.:,;'$\"?!/()#%*&+-=><☺♂♀éâàçêëèïîìôòûùáíóú",
                        m.Value.Get<string>("Mapping", null));
                });
        }

        [Fact]
        public void VerifyTilesets()
        {
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/ICONDAT0.XLD"];
            Assert.Equal("UAlbion.Formats.Parsers.TilesetLoader, UAlbion.Formats", f.Loader);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                    Assert.Equal("tiledata.1", m.Value.Id);
                    Assert.True(m.Value.Get("UseSmallGraphics", false));
                },
                m =>
                {
                    Assert.Equal(1, m.Key);
                    Assert.True(m.Value.Get("UseSmallGraphics", false));
                },
                m =>
                {
                    Assert.Equal(3, m.Key);
                    Assert.True(m.Value.Get("UseSmallGraphics", false));
                });
        }

        [Fact]
        public void VerifyPalettes()
        {
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/PALETTE0.XLD"];
            Assert.Equal("UAlbion.Formats.Parsers.PaletteLoader, UAlbion.Formats", f.Loader);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                    Assert.Equal("pal.1", m.Value.Id);
                    Assert.Collection(m.Value.GetArray<string>("AnimatedRanges"),
                        x => Assert.Equal("0x99-0x9f", x),
                        x => Assert.Equal("0xb0-0xbf", x));
                },
                m =>
                {
                    Assert.Equal(1, m.Key);
                    Assert.Collection(m.Value.GetArray<string>("AnimatedRanges"),
                        x => Assert.Equal("0x99-0x9f", x),
                        x => Assert.Equal("0xb0-0xb4", x),
                        x => Assert.Equal("0xb5-0xbf", x));
                });
        }

        [Fact]
        public void VerifySpells()
        {
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(XLD)/SPELLDAT.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(LANG)/WORDLIS0.XLD"];
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
            var c = AssetConfig.Parse(TestConfig1);
            var f = c.Files["$(ALBION)/MAIN.EXE#476227b0391cf3452166b7a1d52b012ccf6c86bc9e46886dafbed343e9140710"];
            Assert.Equal("UAlbion.Formats.Parsers.FixedSizeSpriteLoader, UAlbion.Formats", f.Loader);
            Assert.Equal("UAlbion.Formats.Containers.BinaryOffsetContainer, UAlbion.Formats", f.Container);
            Assert.Collection(f.Map.OrderBy(x => x.Key),
                m =>
                {
                    Assert.Equal(0, m.Key);
                    Assert.Equal("coresprite.0", m.Value.Id);
                    Assert.Equal(0x0FBE58, m.Value.Get("Offset", 0));
                    Assert.Equal(14, m.Value.Width);
                    Assert.Equal(14, m.Value.Height);
                    var hotspot = m.Value.GetRaw("Hotspot");
                    Assert.Equal(-6, hotspot.Value<int>("X"));
                    Assert.Equal(0, hotspot.Value<int>("Y"));
                },
                m =>
                {
                    Assert.Equal(1, m.Key);
                    Assert.Equal(0x0FBF1C, m.Value.Get("Offset", 0));
                    Assert.Equal(16, m.Value.Width);
                    Assert.Equal(16, m.Value.Height);
                    var hotspot = m.Value.GetRaw("Hotspot");
                    Assert.Equal(0, hotspot.Value<int>("X"));
                    Assert.Equal(4, hotspot.Value<int>("Y"));
                },
                m =>
                {
                    Assert.Equal(27, m.Key);
                    Assert.Equal(0x0FDD10, m.Value.Get("Offset", 0));
                    Assert.Equal(32, m.Value.Width);
                    Assert.Equal(64, m.Value.Height);
                    Assert.Null(m.Value.GetRaw("Hotspot"));
                });
        }
    }
}

