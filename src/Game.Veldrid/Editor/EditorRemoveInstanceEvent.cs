namespace UAlbion.Game.Veldrid.Editor
{
    public class EditorRemoveInstanceEvent : IEditorEvent
    {
        public int Id { get; }
        public string CollectionName { get; }
        public int Index { get; }
        public EditorAsset Asset { get; }
    }
}