using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        PartyCharacterId Id { get; }
        Vector2 Position { get; }
    }
}