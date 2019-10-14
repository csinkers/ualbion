namespace UAlbion.Game.Events
{
    public interface IUiEvent : IGameEvent
    {
        bool Propagating { get; set; }
    }
}