using System.Collections.Generic;

namespace UAlbion.Formats.Exporters.Tiled;

public interface ITiledPropertySource
{
    List<TiledProperty> Properties { get; }
}