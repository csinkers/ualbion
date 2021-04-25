using UAlbion.Core;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Entities.Map3D
{
    public class Collider3D : Component
    {
        readonly LogicalMap3D _logicalMap;

        public Collider3D(LogicalMap3D logicalMap)
        {
            _logicalMap = logicalMap;
        }
    }
}