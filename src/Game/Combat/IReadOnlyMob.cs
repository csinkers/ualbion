using UAlbion.Formats.Assets;

namespace UAlbion.Game.Combat;

public interface IReadOnlyMob
{
    public int X { get; }
    public int Y { get; }
    public IEffectiveCharacterSheet Sheet { get; }
}