using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IMovement : IComponent
    {
        (Vector3, int) GetPositionHistory(PartyCharacterId partyMember);
    }
}