using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps;

public class TilesetData
{
    public const int TileCount = 4097;
    public TilesetData() { }
    public TilesetData(TilesetId id) => Id = id;
    [JsonInclude] public TilesetId Id { get; private set; } // Setter required for JSON
    public bool UseSmallGraphics { get; set; } // Careful if renaming: needs to match up to asset property in assets.json
    [JsonInclude] public List<TileData> Tiles { get; private set; } = new();

    public static TilesetData Serdes(TilesetData td, ISerializer s, AssetInfo info)
    {
        const int dummyTileCount = 1;
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (info == null) throw new ArgumentNullException(nameof(info));

        int tileCount = td?.Tiles.Count ?? (int)(s.BytesRemaining / 8) + dummyTileCount;
        const string expected = @"Chain0:
action StartDialogue
if (in_party PartyMember.Sira) {
    if (get_switch Switch.Switch597) {
        if (get_switch Switch.Switch77) {
            map_text EventText.Sira2 11 StandardOptions
        } else {
            map_text EventText.Sira2 7 StandardOptions
        }
        map_text EventText.Sira2 8 ConversationOptions
        map_text EventText.Sira2 8 Conversation
    } else {
        map_text EventText.Sira2 7 StandardOptions
        map_text EventText.Sira2 1 ConversationOptions
        map_text EventText.Sira2 1 Conversation
    }
} else {
    if (get_switch Switch.Switch76) {
        map_text EventText.Sira2 16
    } else {
        map_text EventText.Sira2 15
    }
    switch Set Switch.Switch76
    if (prompt_player EventText.Sira2 17) {
        map_text EventText.Sira2 19
        add_party_member PartyMember.Sira 3
        if (result) {
            add_party_member PartyMember.Mellthas 3
            if (result) {
                npc_disabled 8 1 0 281
                npc_disabled 9 1 0 281
                end_dialogue
            } else {
                L1:
                map_text EventText.Sira2 18
                remove_party_member PartyMember.Sira 1 26888
                remove_party_member PartyMember.Mellthas 1 26889
            }
        } else {
            goto L1
        }
    } else {
        map_text EventText.Sira2 20
        end_dialogue
    }
}";
        td ??= new TilesetData(info.AssetId);
        td.UseSmallGraphics = info.Get(AssetProperty.UseSmallGraphics, td.UseSmallGraphics);

        if (td.Tiles.Count == 0)
        {
            td.Tiles.Add(new TileData
            {
                Layer = TileLayer.Normal,
                Type = TileType.Normal,
                Collision = Passability.Passable,
                ImageNumber = 0xffff,
                FrameCount = 1,
                Unk7 = 0
            });
        }

        s.List(nameof(Tiles), td.Tiles, tileCount - dummyTileCount, dummyTileCount, S.Object<TileData>(TileData.Serdes));

        if (s.IsReading())
            for (ushort i = 0; i < td.Tiles.Count; i++)
                td.Tiles[i].Index = i;

        return td;
    }
}