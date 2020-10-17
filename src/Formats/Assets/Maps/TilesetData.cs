using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps
{
    public class TilesetData
    {
        public bool UseSmallGraphics { get; set; }
        public IList<TileData> Tiles { get; } = new List<TileData>();

        public static TilesetData Serdes(TilesetData td, ISerializer s, AssetInfo config)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            int tileCount = td?.Tiles.Count ?? (int)(s.BytesRemaining / 8) + 2;
            td ??= new TilesetData();
            td.UseSmallGraphics = config?.UseSmallGraphics ?? td.UseSmallGraphics;

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

            s.List(nameof(Tiles), td.Tiles, (tileCount - 2), 2, S.Object<TileData>(TileData.Serdes));
            return td;
        }
    }
}
