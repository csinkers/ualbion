using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Entities;
using UAlbion.Game.Gui;

namespace UAlbion.Game
{
    public class GameState
    {
        IDictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        Party _party;
        GameFrame _frame;
        DateTime _time;
    }
}
