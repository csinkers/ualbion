using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game;

namespace UAlbion
{
    [Event("isopos")] public class IsoPosEvent : Event { }
    public class IsometricLayout : Component
    {
        readonly LabyrinthData _labyrinthData;
        readonly PaletteId _paletteId;
        readonly ushort _width;
        readonly ushort _height;
        readonly byte[] _contents;
        readonly byte[] _floors;
        readonly byte[] _ceilings;
        readonly Vector3 _tileSize;
        readonly Vector2 _tileSpacing;
        DungeonTileMap _tilemap;

        public IsometricLayout(LabyrinthData labyrinthData, PaletteId paletteId, Vector3 tileSize, Vector2 tileSpacing)
        {
            _labyrinthData = labyrinthData ?? throw new ArgumentNullException(nameof(labyrinthData));
            _paletteId = paletteId;
            _tileSize = tileSize;
            _tileSpacing = tileSpacing;
            int totalTiles = 2 * labyrinthData.FloorAndCeilings.Count + labyrinthData.Walls.Count;
            _width = (ushort)ApiUtil.NextPowerOfTwo((int)Math.Ceiling(Math.Sqrt(totalTiles)));
            _height = (ushort)((totalTiles + _width - 1) / _width);
            // _width *= 2;
            // _height *= 2;
            _floors = new byte[_width * _height];
            _ceilings = new byte[_width * _height];
            _contents = new byte[_width * _height];

            int index = 0;
            for (byte i = 0; i < labyrinthData.FloorAndCeilings.Count; i++) _floors[Map(index++, _width)] = i;
            for (byte i = 0; i < labyrinthData.FloorAndCeilings.Count; i++) _ceilings[Map(index++, _width)] = i;
            for (byte i = 0; i < labyrinthData.Walls.Count; i++) _contents[Map(index++, _width)] = (byte)(i + 100);

            On<IsoPosEvent>(_ =>
            {
                var camera = Resolve<ICamera>();
                var v1 = camera.ProjectWorldToNorm(Vector3.Zero);
                var v2 = new Vector3(tileSize.X * _width, tileSize.Y, tileSize.Z * _height);
                var v3 = camera.ProjectWorldToNorm(v2);

                var msg = $"W:{_width} H:{_height} Tot:{totalTiles} CPos:{camera.Position} CDir:{camera.LookDirection} CYaw:{camera.Yaw} CPitch:{camera.Pitch}";
                Raise(new LogEvent(LogEvent.Level.Info, msg));
                Raise(new LogEvent(LogEvent.Level.Info, $"{Vector3.Zero} => {v1}, tile = {tileSize}"));
                Raise(new LogEvent(LogEvent.Level.Info, $"{v2} => {v3}"));
            });
        }

        static int Map(int i, int width) => i;
        // {
        //     int x = i % (width / 2);
        //     int y = i / (width / 2);
        //     return 2 * x + 2 * y * width;
        // }

        protected override void Subscribed()
        {
            Raise(new LoadPaletteEvent(_paletteId));

            if (_tilemap != null)
                return;

            var assets = Resolve<IAssetManager>();
            _tilemap = new DungeonTileMap(_labyrinthData.Id, 
                _labyrinthData.Id.ToString(),
                DrawLayer.Background,
                _tileSize,
                _width, _height,
                Resolve<ICoreFactory>(),
                Resolve<IPaletteManager>());

            for(int i = 0; i < _labyrinthData.FloorAndCeilings.Count; i++)
            {
                var floorInfo = _labyrinthData.FloorAndCeilings[i];
                ITexture floor = assets.LoadTexture(floorInfo?.SpriteId ?? AssetId.None);
                _tilemap.DefineFloor(i + 1, floor);
            }

            for (int i = 0; i < _labyrinthData.Walls.Count; i++)
            {
                var wallInfo = _labyrinthData.Walls[i];
                if (wallInfo == null)
                    continue;

                ITexture wall = assets.LoadTexture(wallInfo.SpriteId);
                if (wall == null)
                    continue;

                bool isAlphaTested = (wallInfo.Properties & Wall.WallFlags.AlphaTested) != 0;
                _tilemap.DefineWall(i + 1, wall, 0, 0, wallInfo.TransparentColour, isAlphaTested);

                foreach(var overlayInfo in wallInfo.Overlays)
                {
                    if (overlayInfo.SpriteId.IsNone)
                        continue;

                    var overlay = assets.LoadTexture(overlayInfo.SpriteId);
                    _tilemap.DefineWall(i + 1, overlay, overlayInfo.XOffset, overlayInfo.YOffset, wallInfo.TransparentColour, isAlphaTested);
                }
            }

            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    int index = j * _width + i;
                    SetTile(index, index, 0); // TODO: Frames
                }
            }

            Resolve<IEngine>()?.RegisterRenderable(_tilemap);
        }

        protected override void Unsubscribed()
        {
            Resolve<IEngine>()?.UnregisterRenderable(_tilemap);
            _tilemap = null;
        }

        void SetTile(int index, int order, int frameCount)
        {
            byte i = (byte)(index % _width);
            byte j = (byte)(index / _width);
            byte floorIndex = _floors[index];
            byte ceilingIndex = _ceilings[index];
            int contents = _contents[index];
            byte wallIndex = (byte)(contents < 100 || contents - 100 >= _labyrinthData.Walls.Count
                ? 0
                : contents - 100);

            _tilemap.Set(order, i * _tileSpacing.X / _tileSize.X, j * _tileSpacing.Y / _tileSize.Z, floorIndex, ceilingIndex, wallIndex, frameCount);
        }
    }
}