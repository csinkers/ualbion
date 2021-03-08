using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;

namespace UAlbion.Game.Assets
{
    public class TiledTilesetLoader : Component, IAssetLoader<TilesetData>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((TilesetData) existing, config, mapping, s);

        public TilesetData Serdes(TilesetData existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));

            if(s.IsWriting())
            {
                if(existing == null) throw new ArgumentNullException(nameof(existing));
                var graphicsId = ((TilesetId)config.AssetId).ToTilesetGraphics();
                var graphicsTemplate = config.Get(AssetProperty.GraphicsPattern, "{0}/{0}_{1}.png");
                var blankTilePath = config.Get(AssetProperty.BlankTilePath, "Blank.png");

                var properties = new TilemapProperties
                {
                    GraphicsTemplate = graphicsTemplate,
                    BlankTilePath = blankTilePath,
                    TilesetId = graphicsId.Id,
                    TileWidth = 16,
                    TileHeight = 16
                };

                var assets = Resolve<IAssetManager>();
                var graphicsInfo = assets.GetAssetInfo(existing.Id.ToTilesetGraphics());
                var tiledTileset = Tileset.FromTileset(existing, properties, graphicsInfo);
                var bytes = FormatUtil.BytesFromTextWriter(tiledTileset.Serialize);
                s.ByteArray(null, bytes, bytes.Length);

                return existing;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}