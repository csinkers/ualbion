using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D
{
    public class MapRenderable2D : Component
    {
        readonly LogicalMap2D _logicalMap;
        readonly IMapLayer _underlay;
        readonly IMapLayer _overlay;
        readonly InfoOverlay _info;
        readonly MapAnnotationLayer _annotations;

        public MapRenderable2D(LogicalMap2D logicalMap, ITexture tileset, IGameFactory factory)
        {
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
            var subImage = tileset.Regions[0];
            TileSize = subImage.Size;

            _underlay = AttachChild(factory.CreateMapLayer(
                logicalMap,
                tileset,
                logicalMap.GetUnderlay,
                DrawLayer.Underlay,
                IconChangeType.Underlay));

            _overlay = AttachChild(factory.CreateMapLayer(logicalMap,
                tileset,
                logicalMap.GetOverlay,
                DrawLayer.Overlay,
                IconChangeType.Overlay));

            _info = AttachChild(new InfoOverlay(logicalMap));

            var tileSize = tileset.Regions[0].Size;
            _annotations = AttachChild(new MapAnnotationLayer(logicalMap, tileSize));

            On<ToggleUnderlayEvent>(e => _underlay.IsActive = !_underlay.IsActive);
            On<ToggleOverlayEvent>(e => _overlay.IsActive = !_overlay.IsActive);
        }

        public Vector2 TileSize { get; }
        public PaletteId Palette => _logicalMap.PaletteId;
        public Vector2 SizePixels => new Vector2(_logicalMap.Width, _logicalMap.Height) * TileSize;
        public IWeakSpriteReference GetWeakUnderlayReference(int x, int y) => _underlay.GetWeakSpriteReference(x, y);
        public IWeakSpriteReference GetWeakOverlayReference(int x, int y) => _overlay.GetWeakSpriteReference(x, y);

        public void SetHighlightIndex(int? index)
        {
            _underlay.HighlightIndex = index;
            _overlay.HighlightIndex = index;
        }

        protected override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
    }
}
