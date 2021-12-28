using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using Xunit;

namespace UAlbion.Formats.Tests;

public class JsonTests
{
    [Fact]
    public void CustomDictionaryTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.Npc), AssetType.Npc)
            .RegisterAssetType(typeof(Base.PartyMember), AssetType.Party)
            ;

        var sheets = new Dictionary<CharacterId, CharacterSheet>
        {
            { Base.Npc.Argim, new CharacterSheet(Base.Npc.Argim) },
            { Base.PartyMember.Tom, new CharacterSheet(Base.PartyMember.Tom) }
        };

        var jsonUtil = new JsonUtil();
        var json = jsonUtil.Serialize(sheets);
        var reloaded = jsonUtil.Deserialize<Dictionary<CharacterId, CharacterSheet>>(json);

        Assert.Collection(reloaded.OrderBy(x => x.Key.ToString()),
            kvp =>
            {
                Assert.Equal(Base.Npc.Argim, kvp.Key);
            },
            kvp =>
            {
                Assert.Equal(Base.PartyMember.Tom, kvp.Key);
            }
        );
    }

    [Fact]
    public void ItemSlotTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.PartyMember), AssetType.Party)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;
        var id = new InventorySlotId((CharacterId) Base.PartyMember.Tom, ItemSlotId.Head);
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

            var parsed = ItemSlot.Parse(expected[i]);
            var reprint = parsed.ToString();
            Assert.Equal(reprint, tests[i].ToString());
        }
    }

    const string InventoryJson = @"
{
    ""Id"": ""Monster.Magician1"",
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
            .RegisterAssetType(typeof(Base.Monster), AssetType.Monster)
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
            .RegisterAssetType(typeof(Base.Monster), AssetType.Monster)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            ;

        var options = new JsonSerializerOptions { Converters = { InventoryConverter.Instance } };

        var inv = new Inventory(new InventoryId((MonsterId)Base.Monster.Magician1));
        inv.Gold.Item = Gold.Instance;
        inv.Gold.Amount = 20;
        inv.Rations.Item = Rations.Instance;
        inv.Rations.Amount = 1;
        inv.Slots[3].ItemId = Base.Item.Torch;
        inv.Slots[3].Amount = 3;
        inv.Slots[4].ItemId = Base.Item.Clothes;
        inv.Slots[7].ItemId = Base.Item.Shoes;

        var json = JsonSerializer.Serialize(inv, options);
        var roundTripped = JsonSerializer.Deserialize<Inventory>(json, options);

        Assert.NotNull(roundTripped);
        Assert.Equal(20, roundTripped.Gold.Amount);
        Assert.Equal(1, roundTripped.Rations.Amount);
        Assert.Equal(Base.Item.Torch, roundTripped.Slots[3].ItemId);
        Assert.Equal(3, roundTripped.Slots[3].Amount);
        Assert.Equal(Base.Item.Clothes, roundTripped.Slots[4].ItemId);
        Assert.Equal(Base.Item.Shoes, roundTripped.Slots[7].ItemId);
    }
}