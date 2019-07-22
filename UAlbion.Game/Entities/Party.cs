using System.Collections.Generic;

namespace UAlbion.Game.Entities
{
    public class Party
    {
        const int PositionHistoryCount = 40;

        readonly IList<Player> _players = new List<Player>();
        readonly (int,int)[] _positions = new (int,int)[PositionHistoryCount];
        Direction _facing;
        int _activePlayer;
        bool _hasClock;
        bool _hasProximityDetector;
    }
}
