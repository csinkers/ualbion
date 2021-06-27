using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public interface IEtmManager
    {
        IEnumerable<IDungeonTilemap> Ordered { get; }
        IDungeonTilemap CreateTilemap(TilemapRequest request);
    }
}