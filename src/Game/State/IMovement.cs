using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.State;

public interface IMovement : IComponent
{
    (Vector3, int) GetPositionHistory(int followerIndex);
}