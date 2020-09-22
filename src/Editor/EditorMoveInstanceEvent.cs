namespace UAlbion.Editor
{
    public class EditorMoveInstanceEvent : IEditorEvent
    {
        public int Id { get; }
        public string CollectionName { get; }
        public int FromIndex { get; }
        public int ToIndex { get; }
    }
}