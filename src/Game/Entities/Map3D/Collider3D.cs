using UAlbion.Api.Eventing;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Entities.Map3D;

public class Collider3D(LogicalMap3D logicalMap) : Component
{
    // ReSharper disable once UnusedMember.Local
    readonly LogicalMap3D _logicalMap = logicalMap;
}