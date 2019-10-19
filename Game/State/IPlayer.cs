namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        string Name { get; }
        CharacterSheet Stats { get; }
    }
}