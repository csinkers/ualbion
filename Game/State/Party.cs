using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public class Party : IParty
    {
        public const int MaxPartySize = 6;
        readonly List<Player.Player> _players = new List<Player.Player>();
        public IList<Player.Player> Players => _players;  // Max of 6

        // The current party leader (shown with a white outline on
        // health bar and slightly raised in the status bar)
        public PartyCharacterId Leader
        {
            set
            {
                int index = _players.FindIndex(x => x.Id == value);
                if (index == -1) 
                    return;

                var player = _players[index];
                _players.RemoveAt(index);
                _players.Insert(0, player);
            }
        }

        IReadOnlyList<IPlayer> IParty.Players => _players;  // Max of 6
    }

    public interface IMovement : IComponent
    {
        (Vector2, int) GetPositionHistory(PartyCharacterId partyMember);
    }
}
