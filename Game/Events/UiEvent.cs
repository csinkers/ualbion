namespace UAlbion.Game.Events
{
    public abstract class UiEvent : GameEvent, IUiEvent
    {
        public bool Propagating { get; set; } = true;
    }
}