using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Maps;

public class TilesetData
{
    public const int TileCount = 4097;
    public TilesetData() { }
    public TilesetData(TilesetId id) => Id = id;
    [JsonInclude] public TilesetId Id { get; private set; } // Setter required for JSON
    public bool UseSmallGraphics { get; set; } // Careful if renaming: needs to match up to asset property in assets.json
    [JsonInclude] public List<TileData> Tiles { get; private set; } = [];

    public static TilesetData Serdes(TilesetData td, ISerializer s, AssetLoadContext context)
    {
        const int dummyTileCount = 1;
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(context);

        int tileCount = td?.Tiles.Count ?? (int)(s.BytesRemaining / 8) + dummyTileCount;
        td ??= new TilesetData(context.AssetId);
        td.UseSmallGraphics = context.GetProperty(TilesetLoader.UseSmallGraphicsProperty, td.UseSmallGraphics);

        if (td.Tiles.Count == 0)
        {
            td.Tiles.Add(new TileData
            {
                Layer = TileLayer.Normal,
                Type = TileType.Unk0,
                Collision = Passability.Open,
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