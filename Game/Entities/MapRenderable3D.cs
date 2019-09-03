using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable3D : Component
    {
        readonly TileMap _tilemap;
        readonly MapData3D _mapData;
        readonly LabyrinthData _labyrinthData;
        int _frameCount;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MapRenderable3D, RenderEvent>((x, e) => x.Render(e)),
            new Handler<MapRenderable3D, UpdateEvent>((x, e) => x.Update(e)),
            new Handler<MapRenderable3D, SubscribedEvent>((x, e) => x.Subscribed())
        };

        public MapRenderable3D(Assets assets, MapData3D mapData, LabyrinthData labyrinthData) : base(Handlers)
        {
            _mapData = mapData;
            _labyrinthData = labyrinthData;
            var palette = assets.LoadPalette(_mapData.PaletteId);
            _tilemap = new TileMap(
                (int)DrawLayer.Background, 
                Vector3.One * 64.0f, 
                _mapData.Width, _mapData.Height, 
                palette.GetCompletePalette());

            for(int i = 0; i < _labyrinthData.FloorAndCeilings.Count; i++)
            {
                var floorInfo = _labyrinthData.FloorAndCeilings[i];
                ITexture floor = floorInfo?.TextureNumber == null ? null : assets.LoadTexture(floorInfo.TextureNumber.Value);
                _tilemap.DefineFloor(i + 1, floor);
            }

            for (int i = 0; i < _labyrinthData.Walls.Count; i++)
            {
                var wallInfo = _labyrinthData.Walls[i];
                if (wallInfo?.TextureNumber == null)
                    continue;

                ITexture wall = assets.LoadTexture(wallInfo.TextureNumber.Value);
                _tilemap.DefineWall(i + 1, wall, 0, 0);

                foreach(var overlayInfo in wallInfo.Overlays)
                {
                    if (!overlayInfo.TextureNumber.HasValue)
                        continue;

                    var overlay = assets.LoadTexture(overlayInfo.TextureNumber.Value);
                    _tilemap.DefineWall(i + 1, overlay, overlayInfo.XOffset, overlayInfo.YOffset);
                }
            }

            Update(new UpdateEvent(0));
        }

        void Subscribed() { Raise(new LoadPalEvent((int)_mapData.PaletteId)); }

        void Update(UpdateEvent e)
        {
            _frameCount += e.Frames;
            for (int j = 0; j < _mapData.Height; j++)
            {
                for (int i = 0; i < _mapData.Width; i++)
                {
                    int index = j * _mapData.Width + i;
                    byte floorIndex = (byte)_mapData.Floors[index];
                    byte ceilingIndex = (byte)_mapData.Ceilings[index];
                    int contents = _mapData.Contents[index];
                    byte wallIndex = (byte)(contents < 100 || contents - 101 >= _labyrinthData.Walls.Count ? 0 : contents - 101);

                    _tilemap.Set(i, j, floorIndex, ceilingIndex, wallIndex, _frameCount);
                }
            }
        }

        void Render(RenderEvent e)
        {
            e.Add(_tilemap);
        }
    }
}