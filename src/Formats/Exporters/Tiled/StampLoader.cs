using System;
using System.Globalization;
using System.Text;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class StampLoader : IAssetLoader<BlockList>
    {
        public BlockList Serdes(BlockList existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));

                var blockListId = (BlockListId)info.AssetId;
                var tilesetId = blockListId.ToTileset();

                var tilesetPattern = info.Get(AssetProperty.TilesetPattern, "../Tilesets/{0}_{2}.tsx");
                var tilesetPath = string.Format(CultureInfo.InvariantCulture,
                    tilesetPattern,
                    tilesetId.Id,
                    0,
                    ConfigUtil.AssetName(tilesetId));

                var tileset = new Tileset
                {
                    Filename = tilesetPath,
                    TileWidth = 16,
                    TileHeight = 16,
                };

                PackedChunks.Pack(s, existing.Count, stampNumber =>
                {
                    if (existing[stampNumber].Width == 0 || existing[stampNumber].Height == 0)
                        return Array.Empty<byte>();
                    var stamp = new Stamp(stampNumber, existing[stampNumber], tileset);
                    return FormatUtil.BytesFromTextWriter(stamp.Serialize);
                });

                return existing;
            }

            var list = new BlockList();
            foreach(var jsonBytes in  PackedChunks.Unpack(s))
            {
                var json = Encoding.UTF8.GetString(jsonBytes);
                var stamp = Stamp.Parse(json);
                var block = stamp.ToBlock();
                list.Add(block);
            }

            return list;
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((BlockList) existing, info, mapping, s);

        /* .stamp file
{
  "name":"schr",
  "variations":[
  {
    "map":
    {
      "compressionlevel":-1,
      "height":7,
      "infinite":false,
      "layers":[
      {
        "compression":"zlib",
        "data":"eJxjYMAEW5gZGLYyI/jbgOztSPwdQPZOKH8XkN4NxHuAeC8Q7wPi/UB8AIgPAvEhID4MxEeg6gEpPwyw",
        "encoding":"base64",
        "height":7,
        "id":1,
        "name":"Under",
        "opacity":1,
        "type":"tilelayer",
        "visible":true,
        "width":4,
        "x":0,
        "y":0
      },
      {
        "compression":"zlib",
        "data":"eJxjYGBgOM3JwHAGiIkBZ5HUnSNSDzIAABF/A1c=",
        "encoding":"base64",
        "height":7,
        "id":2,
        "name":"Over",
        "opacity":1,
        "type":"tilelayer",
        "visible":true,
        "width":4,
        "x":0,
        "y":0
      }],
      "nextlayerid":3,
      "nextobjectid":1,
      "orientation":"orthogonal",
      "renderorder":"right-down",
      "tiledversion":"1.4.3",
      "tileheight":16,
      "tilesets":[
      {
        "firstgid":1,
        "source":"../../Tilesets/3_Indoors.tsx"
      }],
      "tilewidth":16,
      "type":"map",
      "version":1.4,
      "width":4
    },
    "probability":1
  }]
}
         */
    }
}