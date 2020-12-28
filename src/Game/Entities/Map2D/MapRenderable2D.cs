using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map2D
{
    public class MapRenderable2D : Component
    {
        readonly LogicalMap2D _logicalMap;
        readonly TileLayer _underlay;
        readonly TileLayer _overlay;
        readonly InfoOverlay _info;
        readonly MapAnnotationLayer _annotations;

        public MapRenderable2D(LogicalMap2D logicalMap, ITexture tileset)
        {
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
            var subImage = tileset.GetSubImageDetails(0);
            TileSize = subImage.Size;

            _underlay = AttachChild(new TileLayer(
                logicalMap,
                tileset,
                logicalMap.GetUnderlay,
                DrawLayer.Underlay,
                IconChangeType.Underlay));

            _overlay = AttachChild(new TileLayer(logicalMap,
                tileset,
                logicalMap.GetOverlay,
                DrawLayer.Overlay,
                IconChangeType.Overlay));

            _info = AttachChild(new InfoOverlay(logicalMap));

            var tileSize = tileset.GetSubImageDetails(0).Size;
            _annotations = AttachChild(new MapAnnotationLayer(logicalMap, tileSize));

            On<ToggleUnderlayEvent>(e => _underlay.IsActive = !_underlay.IsActive);
            On<ToggleOverlayEvent>(e => _overlay.IsActive = !_overlay.IsActive);
        }

        public Vector2 TileSize { get; }
        public PaletteId Palette => _logicalMap.PaletteId;
        public Vector2 SizePixels => new Vector2(_logicalMap.Width, _logicalMap.Height) * TileSize;
        public WeakSpriteReference GetWeakUnderlayReference(int x, int y) => _underlay.GetWeakSpriteReference(x, y);
        public WeakSpriteReference GetWeakOverlayReference(int x, int y) => _overlay.GetWeakSpriteReference(x, y);

        public void SetHighlightIndex(int? index)
        {
            _underlay.HighlightIndex = index;
            _overlay.HighlightIndex = index;
        }

        protected override void Subscribed() => Raise(new LoadPaletteEvent(Palette));
    }
}
