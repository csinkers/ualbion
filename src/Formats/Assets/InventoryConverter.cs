using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public class InventoryConverter : JsonConverter<Inventory>
{
    /* e.g.
    {
      "Id": "Monster.Magician1",
      "Slots": {
          "3": "3x Item.Torch",
          "4": "Item.Clothes",
          "7": "Item.Shoes",
          "Gold": 2.0,
          "Rations": 1
      }
    } */

    InventoryConverter() { }
    public static readonly InventoryConverter Instance = new();

    static Dictionary<ItemSlotId, ItemSlot> ReadSlots(ref Utf8JsonReader reader, InventoryId inventoryId)
    {
        var slots = new Dictionary<ItemSlotId, ItemSlot>();
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Unexpected token type in Slots property of Inventory: {reader.TokenType}, expected StartObject");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return slots;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token type in Slots object of Inventory: {reader.TokenType}, expected PropertyName");

            ItemSlotId slotId;
            var slotKey = reader.GetString();
            if (int.TryParse(slotKey, out var numId))
                slotId = (ItemSlotId)numId;
            else if (Enum.TryParse(typeof(ItemSlotId), slotKey, true, out var enumId) && enumId != null)
                slotId = (ItemSlotId)enumId;
            else
                throw new JsonException($"Could not parse inventory slot name \"{slotKey}\" as a numerical or named slot id");

            if (!reader.Read())
                throw new JsonException("Unexpected end of file reading Inventory");

            switch (slotId)
            {
                case ItemSlotId.Gold when reader.TokenType != JsonTokenType.Number:
                    throw new JsonException($"Unexpected token when reading gold slot: {reader.TokenType}, expected Number");
                case ItemSlotId.Gold:
                {
                    var amount = (int)(reader.GetDecimal() * 10m);
                    if (amount is < 0 or > ushort.MaxValue)
                        throw new JsonException($"Gold value AssetId.{amount} out of range, max gold: {ushort.MaxValue / 10m}");

                    var goldSlot = new ItemSlot(new InventorySlotId(inventoryId, ItemSlotId.Gold));
                    slots.Add(slotId, goldSlot.Set(AssetId.Gold, (ushort)amount));
                    break;
                }

                case ItemSlotId.Rations when reader.TokenType != JsonTokenType.Number:
                    throw new JsonException($"Unexpected token when reading rations slot: {reader.TokenType}, expected Number");
                case ItemSlotId.Rations:
                {
                    var amount = reader.GetInt32();
                    if (amount is < 0 or > ushort.MaxValue)
                        throw new JsonException($"Rations value {amount} out of range, max rations: {ushort.MaxValue}");

                    var rationSlot = new ItemSlot(new InventorySlotId(inventoryId, ItemSlotId.Rations));
                    slots.Add(slotId, rationSlot.Set(AssetId.Rations, (ushort)amount));
                    break;
                }
                default:
                {
                    if (reader.TokenType != JsonTokenType.String)
                        throw new JsonException($"Unexpected token when reading Inventory slots: {reader.TokenType}, expected String");

                    slots.Add(slotId, ItemSlot.Parse(reader.GetString()));
                    break;
                }
            }
        }

        throw new JsonException("Unexpected end of file reading Inventory");
    }

    public override Inventory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Tried to deserialize a token of type {reader.TokenType} as an Inventory, expected StartObject");

        var invId = new InventoryId();
        Dictionary<ItemSlotId, ItemSlot> slots = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (slots == null)
                    throw new JsonException("Expected a \"Slots\" property when reading Inventory object");

                var result = new Inventory(invId);
                for (int i = 0; i < result.Slots.Length; i++)
                {
                    var slotId = (ItemSlotId)i;
                    if (slots.TryGetValue(slotId, out var slot))
                        result.Slots[i] = slot;
                }
                return result;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token when reading Inventory slots: {reader.TokenType}, expected PropertyName");

            var propName = reader.GetString();
            if (string.IsNullOrEmpty(propName))
                throw new JsonException("Unexpected empty property name when reading Inventory");

            if (!reader.Read())
                throw new JsonException("Unexpected end of file reading Inventory");

            if (propName.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException($"Unexpected token type for slot id of Inventory: {reader.TokenType}, expected string.");

                invId = InventoryId.Parse(reader.GetString());
            }
            else if (propName.Equals("Slots", StringComparison.OrdinalIgnoreCase))
            {
                if (invId.Type == InventoryType.Unknown)
                    throw new JsonException("Expected Id property to precede Slots in Inventory JSON");
                slots = ReadSlots(ref reader, invId);
            }
            else throw new JsonException($"Unexpected property name \"{propName}\" reading Inventory (expected Id, Slots)");
        }

        throw new JsonException("Unexpected end of file reading Inventory");
    }

    public override void Write(Utf8JsonWriter writer, Inventory value, JsonSerializerOptions options)
    {
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (value == null) throw new ArgumentNullException(nameof(value));

        writer.WriteStartObject();
        writer.WritePropertyName("Id");
        writer.WriteStringValue(value.Id.ToString());
        writer.WritePropertyName("Slots");
        writer.WriteStartObject();

        foreach (var slot in value.Slots)
        {
            if (slot.Amount == 0)
                continue;

            writer.WritePropertyName(
                slot.Id.Slot < ItemSlotId.NormalSlotCount
                    ? ((int) slot.Id.Slot).ToString(CultureInfo.InvariantCulture)
                    : slot.Id.Slot.ToString());

            if (slot.Id.Slot == ItemSlotId.Gold)
                writer.WriteNumberValue(slot.Amount/10m);
            else if (slot.Id.Slot == ItemSlotId.Rations)
                writer.WriteNumberValue(slot.Amount);
            else writer.WriteStringValue(slot.ToString());
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
        // writer.WriteStringValue(value.ToString());
    }
}