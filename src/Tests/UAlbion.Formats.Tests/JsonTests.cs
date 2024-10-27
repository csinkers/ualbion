using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using Xunit;

namespace UAlbion.Formats.Tests;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public class JsonTests
{
    [Fact]
    public void CustomDictionaryTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.NpcSheet), AssetType.NpcSheet)
            .RegisterAssetType(typeof(Base.PartySheet), AssetType.PartySheet)
            ;

        var sheets = new Dictionary<SheetId, CharacterSheet>
        {
            { Base.NpcSheet.Argim, new CharacterSheet(Base.NpcSheet.Argim) },
            { Base.PartySheet.Tom, new CharacterSheet(Base.PartySheet.Tom) }
        };

        var jsonUtil = new JsonUtil();
        var json = jsonUtil.Serialize(sheets);
        var reloaded = jsonUtil.Deserialize<Dictionary<SheetId, CharacterSheet>>(json);

        Assert.Collection(reloaded.OrderBy(x => x.Key.ToString()),
            kvp =>
            {
                Assert.Equal(Base.NpcSheet.Argim, kvp.Key);
            },
            kvp =>
            {
                Assert.Equal(Base.PartySheet.Tom, kvp.Key);
            }
        );
    }

    [Fact]
    public void ItemSlotTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.PartySheet), AssetType.PartySheet)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;
        var id = new InventorySlotId((SheetId) Base.PartySheet.Tom, ItemSlotId.Head);
        var tests = new[] {
            new ItemSlot(id),
            new ItemSlot(id).Set(Base.Item.Dagger, 1),
            new ItemSlot(id).Set(Base.Item.Dagger, 2),
            new ItemSlot(id).Set(Base.Item.Dagger, 1, ItemSlotFlags.Cursed | ItemSlotFlags.Broken),
            new ItemSlot(id).Set(Base.Item.SerpentStaff, 1, 0, 10, 1),
            new ItemSlot(id).Set(Base.Item.SerpentStaff, 1, ItemSlotFlags.Unk3, 10, 1),
            new ItemSlot(id).Set(Base.Item.Torch, 0xffff)
        };

        var expected = new[] {
            "Empty",
            "Item.Dagger",
            "2x Item.Dagger",
            "Item.Dagger F(Broken, Cursed)",
            "Item.SerpentStaff C10 E1",
            "Item.SerpentStaff F(Unk3) C10 E1",
            "(Inf) Item.Torch"
        };

        for (int i = 0; i < tests.Length; i++)
        {
            Assert.Equal(expected[i], tests[i].ToString());

            var parsed = ItemSlot.Parse(expected[i], new InventorySlotId());
            var reprint = parsed.ToString();
            Assert.Equal(reprint, tests[i].ToString());
        }
    }

    const string InventoryJson = @"
{
    ""Id"": ""MonsterSheet.Magician1"",
    ""Slots"": {
        ""3"": ""3x Item.Torch"",
        ""4"": ""Item.Clothes"",
        ""7"": ""Item.Shoes"",
        ""Gold"": 2.0,
        ""Rations"": 1
    }
}";

    [Fact]
    public void InventoryReadTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.MonsterSheet), AssetType.MonsterSheet)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;

        var options = new JsonSerializerOptions { Converters = { InventoryConverter.Instance } };
        var inv = JsonSerializer.Deserialize<Inventory>(InventoryJson, options);
        Assert.NotNull(inv);
        Assert.Equal((int)ItemSlotId.FullSlotCount, inv.Slots.Length);
        Assert.Equal("3x Item.Torch", inv.Slots[3].ToString());
        Assert.Equal("Item.Clothes", inv.Slots[4].ToString());
        Assert.Equal("Item.Shoes", inv.Slots[7].ToString());
        Assert.Equal(20, inv.Gold.Amount);
        Assert.Equal(1, inv.Rations.Amount);
    }

    [Fact]
    public void InventoryWriteTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.MonsterSheet), AssetType.MonsterSheet)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;

        var options = new JsonSerializerOptions { Converters = { InventoryConverter.Instance } };

        var inv = new Inventory(new InventoryId((MonsterId)Base.MonsterSheet.Magician1))
        {
            Gold = { Item = AssetId.Gold, Amount = 20 },
            Rations = { Item = AssetId.Rations, Amount = 1 }
        };
        inv.Slots[3].Item = Base.Item.Torch;
        inv.Slots[3].Amount = 3;
        inv.Slots[4].Item = Base.Item.Clothes;
        inv.Slots[7].Item = Base.Item.Shoes;

        var json = JsonSerializer.Serialize(inv, options);
        var roundTripped = JsonSerializer.Deserialize<Inventory>(json, options);

        Assert.NotNull(roundTripped);
        Assert.Equal(20, roundTripped.Gold.Amount);
        Assert.Equal(1, roundTripped.Rations.Amount);
        Assert.Equal(Base.Item.Torch, roundTripped.Slots[3].Item);
        Assert.Equal(3, roundTripped.Slots[3].Amount);
        Assert.Equal(Base.Item.Clothes, roundTripped.Slots[4].Item);
        Assert.Equal(Base.Item.Shoes, roundTripped.Slots[7].Item);
    }
}
