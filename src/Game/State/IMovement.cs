using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.State;

public interface IMovement : IComponent
{
    (Vector3, int) GetPositionHistory(int followerIndex);
}