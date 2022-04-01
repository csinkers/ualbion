using System;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Tests;

public class MockGameConfigProvider : IGameConfigProvider
{
    public GameConfig Game { get; }
    public event EventHandler<EventArgs> GameChanged;
    public MockGameConfigProvider(GameConfig game) => Game = game;
}