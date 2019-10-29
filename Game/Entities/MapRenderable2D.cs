using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable2D : Component
    {
        readonly SpriteInstanceData _blankInstance = new SpriteInstanceData(
            Vector3.Zero, Vector2.Zero, 
            Vector2.Zero, Vector2.Zero, 0, 0);

        readonly MapData2D _mapData;
        readonly ITexture _tileset;
        readonly TilesetData _tileData;
        readonly MultiSprite _underlay;
        readonly MultiSprite _overlay;
        int _frameCount;
        bool _renderUnderlay = true;
        bool _renderOverlay = true;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapRenderable2D, RenderEvent>((x, e) => x.Render(e)),
            H<MapRenderable2D, UpdateEvent>((x, e) => x.Update()),
            H<MapRenderable2D, ToggleUnderlayEvent>((x,e) => x._renderUnderlay = !x._renderUnderlay),
            H<MapRenderable2D, ToggleOverlayEvent>((x,e) => x._renderOverlay = !x._renderOverlay)
        );

        public Vector2 TileSize { get; }
        public PaletteId Palette => (PaletteId)_mapData.PaletteId;
        public Vector2 SizePixels => new Vector2(_mapData.Width, _mapData.Height) * TileSize;
        public int? HighlightIndex { get; set; }
        int? _highLightEvent;

        public MapRenderable2D(MapData2D mapData, ITexture tileset, TilesetData tileData) : base(Handlers)
        {
            _mapData = mapData;
            _tileset = tileset;
            _tileData = tileData;
            TileSize = BuildInstanceData(0, 0, _tileData.Tiles[1], 0).Size;

            var underlay = new SpriteInstanceData[_mapData.Width * _mapData.Height];
            var overlay = new SpriteInstanceData[_mapData.Width * _mapData.Height];

            _underlay = new MultiSprite(new SpriteKey(
                _tileset,
                (int)DrawLayer.Underlay,
                underlay[0].Flags))
            {
                Instances = underlay.ToArray()
            };

            _overlay = new MultiSprite(new SpriteKey(
                _tileset,
                (int)DrawLayer.Overlay3,
                overlay[0].Flags))
            {
                Instances = overlay.ToArray()
            };
        }

        protected override void Subscribed()
        {
            Raise(new LoadPaletteEvent(Palette));
            Update();
        }

        SpriteInstanceData BuildInstanceData(int i, int j, TilesetData.TileData tile, int tickCount)
        {
            if (tile == null || tile.Flags.HasFlag(TilesetData.TileFlags.Debug))
                return _blankInstance;

            int index = j * _mapData.Width + i;
            int subImage = tile.GetSubImageForTile(tickCount);

            _tileset.GetSubImageDetails(
                subImage,
                out var tileSize,
                out var texPosition,
                out var texSize,
                out var layer);

            DrawLayer drawLayer = ((int) tile.Layer & 0x7) switch
            {
                (int)TilesetData.TileLayer.Normal => DrawLayer.Underlay,
                (int)TilesetData.TileLayer.Layer1 => DrawLayer.Overlay1,
                (int)TilesetData.TileLayer.Layer2 => DrawLayer.Overlay2,
                (int)TilesetData.TileLayer.Layer3 => DrawLayer.Overlay3,
                _ => DrawLayer.Underlay
            };

            var instance = new SpriteInstanceData
            {
                Offset = new Vector3(
                    new Vector2(i, j) * tileSize,
                    drawLayer.ToZCoordinate(j)),
                Size = tileSize,
                TexPosition = texPosition,
                TexSize = texSize,
                TexLayer = layer
            };

            int zoneNum = _mapData.ZoneLookup[index];
            int eventNum = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;

            instance.Flags =
                (_tileset is EightBitTexture ? SpriteFlags.UsePalette : 0)
                | (HighlightIndex == index ? SpriteFlags.Highlight : 0)
                //| (eventNum != -1 && _highLightEvent != eventNum ? SpriteFlags.Highlight : 0)
                | (_highLightEvent == eventNum ? SpriteFlags.GreenTint : 0)
                //| ((tile.Flags & TilesetData.TileFlags.TextId) != 0 ? SpriteFlags.RedTint : 0)
                //| (((int) tile.Type) == 8 ? SpriteFlags.GreenTint : 0)
                //| (((int) tile.Type) == 12 ? SpriteFlags.BlueTint : 0)
                //| (((int) tile.Type) == 14 ? SpriteFlags.GreenTint | SpriteFlags.RedTint : 0) //&& tickCount % 2 == 0 ? SpriteFlags.Transparent : 0)
                ;

            return instance;
        }

        void Update()
        {
            if (HighlightIndex.HasValue)
            {
                int zoneNum = _mapData.ZoneLookup[HighlightIndex.Value];
                _highLightEvent = zoneNum == -1 ? -1 : _mapData.Zones[zoneNum].EventNumber;
                if (_highLightEvent == -1)
                    _highLightEvent = null;
            }
            else _highLightEvent = null;

            int underlayIndex = 0;
            int overlayIndex = 0;
            for (int j = 0; j < _mapData.Height; j++)
            {
                for (int i = 0; i < _mapData.Width; i++)
                {
                    var underlayTileId = _mapData.Underlay[j * _mapData.Width + i];
                    var underlayTile = underlayTileId == -1 ? null : _tileData.Tiles[underlayTileId];
                    _underlay.Instances[underlayIndex] =
                        BuildInstanceData(i, j, underlayTile, 3 * _frameCount / 2);
                    underlayIndex++;

                    var overlayTileId = _mapData.Overlay[j * _mapData.Width + i];
                    var overlayTile = overlayTileId == -1 ? null : _tileData.Tiles[overlayTileId];
                    _overlay.Instances[overlayIndex] =
                        BuildInstanceData(i, j, overlayTile, 3 * _frameCount / 2);
                    overlayIndex++;
                }
            }

            _frameCount++;
        }

        void Render(RenderEvent e)
        {
            if (_renderUnderlay)
                e.Add(_underlay);
            if (_renderOverlay)
                e.Add(_overlay);
        }
    }
}