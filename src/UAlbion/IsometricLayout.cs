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
    public class IsometricLayout : Component
    {
        DungeonTilemap _tilemap;
        byte[] _contents;
        byte[] _floors;
        byte[] _ceilings;
        int _wallCount;
        int _totalTiles;

        public DungeonTileMapProperties Properties
        {
            get => _tilemap.Properties;
            set => _tilemap.Properties = value;
        }

        protected override void Subscribed()
        {
            if (_tilemap != null)
                Resolve<IEngine>()?.RegisterRenderable(_tilemap);
        }

        public void Load(LabyrinthId labyrinthId, DungeonTileMapProperties properties)
        {
            var engine = Resolve<IEngine>();
            var assets = Resolve<IAssetManager>();
            var coreFactory = Resolve<ICoreFactory>();
            var paletteManager = Resolve<IPaletteManager>();

            if (_tilemap != null)
                engine.UnregisterRenderable(_tilemap);

            var labyrinthData = assets.LoadLabyrinthData(labyrinthId);
            if (labyrinthData == null)
                return;

            var info = assets.GetAssetInfo(labyrinthId);
            int paletteId = info.Get(AssetProperty.PaletteId, 0);
            Raise(new LoadPaletteEvent(new PaletteId(AssetType.Palette, paletteId)));

            _totalTiles = 2 * labyrinthData.FloorAndCeilings.Count + labyrinthData.Walls.Count;
            _wallCount = labyrinthData.Walls.Count;
            _floors = new byte[_totalTiles];
            _ceilings = new byte[_totalTiles];
            _contents = new byte[_totalTiles];

            int index = 0;
            for (byte i = 0; i < labyrinthData.FloorAndCeilings.Count; i++) _floors[index++] = i;
            for (byte i = 0; i < labyrinthData.FloorAndCeilings.Count; i++) _ceilings[index++] = i;
            for (byte i = 0; i < labyrinthData.Walls.Count; i++) _contents[index++] = (byte)(i + 100);

            _tilemap = new DungeonTilemap(labyrinthData.Id, labyrinthData.Id.ToString(), _totalTiles, properties, coreFactory, paletteManager)
            {
                PipelineId = (int)DungeonTilemapPipeline.NoCulling
            };

            for (int i = 0; i < labyrinthData.FloorAndCeilings.Count; i++)
            {
                var floorInfo = labyrinthData.FloorAndCeilings[i];
                var floor = assets.LoadTexture(floorInfo?.SpriteId ?? AssetId.None);
                _tilemap.DefineFloor(i + 1, floor);
            }

            for (int i = 0; i < labyrinthData.Walls.Count; i++)
            {
                var wallInfo = labyrinthData.Walls[i];
                if (wallInfo == null)
                    continue;

                ITexture wall = assets.LoadTexture(wallInfo.SpriteId);
                if (wall == null)
                    continue;

                bool isAlphaTested = (wallInfo.Properties & Wall.WallFlags.AlphaTested) != 0;
                _tilemap.DefineWall(i + 1, wall, 0, 0, wallInfo.TransparentColour, isAlphaTested);

                foreach (var overlayInfo in wallInfo.Overlays)
                {
                    if (overlayInfo.SpriteId.IsNone)
                        continue;

                    var overlay = assets.LoadTexture(overlayInfo.SpriteId);
                    _tilemap.DefineWall(i + 1, overlay, overlayInfo.XOffset, overlayInfo.YOffset, wallInfo.TransparentColour, isAlphaTested);
                }
            }

            for (int i = 0; i < _totalTiles; i++)
                SetTile(i, i, 0);

            engine.RegisterRenderable(_tilemap);
        }

        protected override void Unsubscribed()
        {
            if (_tilemap != null)
                Resolve<IEngine>().UnregisterRenderable(_tilemap);
        }

        void SetTile(int index, int order, int frameCount)
        {
            byte floorIndex = _floors[index];
            byte ceilingIndex = _ceilings[index];
            int contents = _contents[index];
            byte wallIndex = (byte)(contents < 100 || contents - 100 >= _wallCount
                ? 0
                : contents - 100);

            _tilemap.Set(order, floorIndex, ceilingIndex, wallIndex, frameCount);
        }
    }
}