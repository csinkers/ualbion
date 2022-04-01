using System;

namespace UAlbion.Formats.Config;

public interface IGameConfigProvider
{
    GameConfig Game { get; }
    event EventHandler<EventArgs> GameChanged;
}