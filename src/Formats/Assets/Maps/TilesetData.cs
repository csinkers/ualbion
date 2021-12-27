using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps
{
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
            td ??= new TilesetData(info.AssetId);
            td.UseSmallGraphics = info.Get(AssetProperty.UseSmallGraphics, td.UseSmallGraphics);

            if (td.Tiles.Count == 0)
            {
                td.Tiles.Add(new TileData
                {
                    Layer = TileLayer.Normal,
                    Type = TileType.Normal,
                    Collision = Passability.Passable,
                    Flags = 0,
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
}
