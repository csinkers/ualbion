using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable3D : Component
    {
        readonly Assets _assets;
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
            _assets = assets;
            _mapData = mapData;
            _labyrinthData = labyrinthData;
            _tilemap = new TileMap((int)DrawLayer.Background, Vector3.One * 64.0f, _mapData.Width, _mapData.Height);
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
                    var floorInfo = _labyrinthData.FloorAndCeilings[_mapData.Floors[index]];
                    var ceilingInfo = _labyrinthData.FloorAndCeilings[_mapData.Ceilings[index]];
                    var contents = _mapData.Contents[index];
                    var wallInfo = contents < 100 ? _labyrinthData.Walls[contents] : null;

                    DungeonFloorId floorId = (DungeonFloorId)floorInfo.TextureNumber;
                    DungeonFloorId ceilingId = (DungeonFloorId)ceilingInfo.TextureNumber;
                    DungeonWallId wallId = (DungeonWallId)(wallInfo?.TextureNumber ?? 0);
                    //DungeonOverlayId overlayId = (DungeonOverlayId)wallInfo.Overlays.First().;
                    var floor = _assets.LoadTexture(floorId);
                    var ceiling = _assets.LoadTexture(ceilingId);
                    var wall = _assets.LoadTexture(wallId);
                    ITexture overlay = null;
                    //var overlay = _assets.LoadTexture(overlayId);

                    _tilemap.Set(i, j, floor, ceiling, wall, overlay, 0, 0, 0, 0);
                }
            }
        }

        void Render(RenderEvent e)
        {
            e.Add(_tilemap);
        }
    }
}