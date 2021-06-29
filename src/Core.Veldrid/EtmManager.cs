using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public class EtmManager : ServiceComponent<IEtmManager>, IEtmManager
    {
        public IEnumerable<IDungeonTilemap> Ordered => Children.OfType<IDungeonTilemap>();

        public IDungeonTilemap CreateTilemap(TilemapRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Id == null) throw new ArgumentException("The tilemap request did not have an id set", nameof(request));

            var properties = new DungeonTileMapProperties(
                request.Scale, request.Rotation, request.Origin,
                request.HorizontalSpacing, request.VerticalSpacing,
                request.Width,
                request.AmbientLightLevel, request.FogColor,
                request.ObjectYScaling);

            return AttachChild(new DungeonTilemap(this, request.Id, request.Id.ToString(), request.TileCount, properties, request.DayPalette, request.NightPalette));
        }

        public void DisposeTilemap(DungeonTilemap tilemap)
        {
            if (tilemap == null) throw new ArgumentNullException(nameof(tilemap));
            RemoveChild(tilemap);
        }
    }
}