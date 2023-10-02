using System.Collections.Generic;

namespace UAlbion.Game.Combat;

public interface IReadOnlyBattle
{
    IReadOnlyList<IReadOnlyMob> Mobs { get; }
    IReadOnlyMob GetTile(int x, int y);
    IReadOnlyMob GetTile(int tileIndex);
}