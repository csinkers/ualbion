namespace UAlbion.Editor
{
    public class EditorSetPropertyEvent : IEditorEvent
    {
        public EditorSetPropertyEvent(int id, string propertyName, object currentValue, object newValue)
        {
            Id = id;
            PropertyName = propertyName;
            CurrentValue = currentValue;
            NewValue = newValue;
        }

        public int Id { get; }
        public string PropertyName { get; }
        public object CurrentValue { get; }
        public object NewValue { get; }
    }
}