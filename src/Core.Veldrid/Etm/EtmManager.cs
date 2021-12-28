using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Etm;

public class EtmManager : ServiceComponent<IEtmManager>, IEtmManager, IRenderableSource
{
    public IExtrudedTilemap CreateTilemap(TilemapRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.Id == null) throw new ArgumentException("The tilemap request did not have an id set", nameof(request));

        var properties = new DungeonTileMapProperties(
            request.Scale, request.Rotation, request.Origin,
            request.HorizontalSpacing, request.VerticalSpacing,
            request.Width,
            request.AmbientLightLevel, request.FogColor,
            request.ObjectYScaling);

        var result = new ExtrudedTilemap(
            this,
            request.Id,
            request.Id.ToString(),
            request.TileCount,
            properties,
            request.DayPalette,
            request.NightPalette)
        {
            RendererId = request.Pipeline
        };

        AttachChild(result);
        return result;
    }

    public void DisposeTilemap(ExtrudedTilemap tilemap)
    {
        if (tilemap == null) throw new ArgumentNullException(nameof(tilemap));
        RemoveChild(tilemap);
    }

    public void Collect(List<IRenderable> renderables)
    {
        if (renderables == null) throw new ArgumentNullException(nameof(renderables));
        foreach (var child in Children)
        {
            if (child is not ExtrudedTilemap tilemap)
                continue;
            if ((tilemap.OpaqueWindow?.ActiveCount ?? 0) > 0)
                renderables.Add(tilemap.OpaqueWindow);
            if ((tilemap.AlphaWindow?.ActiveCount ?? 0) > 0)
                renderables.Add(tilemap.AlphaWindow);
        }
    }
}