using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Exporters.Tiled
{

    public class StampLoader : IAssetLoader<BlockList>
    {
        public BlockList Serdes(BlockList existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            throw new NotImplementedException();
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((BlockList) existing, config, mapping, s);

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