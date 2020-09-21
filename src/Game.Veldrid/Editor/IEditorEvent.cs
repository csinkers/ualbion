using UAlbion.Api;

namespace UAlbion.Game.Veldrid.Editor
{
    public interface IEditorEvent : IEvent
    {
        int Id { get; }
    }
}