using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Entities;
using UAlbion.Game.Gui;

namespace UAlbion.Game
{
    public class GameState
    {
        IDictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        AlbionPalette _palette;
        Party _party;
        GameFrame _frame;
        DateTime _time;
    }
}
