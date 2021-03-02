﻿using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps
{
    public class TilesetData
    {
        public TilesetData() { }
        public TilesetData(TilesetId id) => Id = id;
        public TilesetId Id { get; private set; }
        public bool UseSmallGraphics { get; set; } // Careful if renaming: needs to match up to asset property in assets.json
        public IList<TileData> Tiles { get; private set; } = new List<TileData>();

        public static TilesetData Serdes(TilesetData td, ISerializer s, AssetInfo config)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));

            int tileCount = td?.Tiles.Count ?? (int)(s.BytesRemaining / 8) + 2;
            td ??= new TilesetData(config.AssetId);
            td.UseSmallGraphics = config.Get(AssetProperty.UseSmallGraphics, td.UseSmallGraphics);

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

            s.List(nameof(Tiles), td.Tiles, tileCount - 2, 2, S.Object<TileData>(TileData.Serdes));

            if (s.IsReading())
                for (ushort i = 0; i < td.Tiles.Count; i++)
                    td.Tiles[i].Index = i;

            return td;
        }
    }
}
