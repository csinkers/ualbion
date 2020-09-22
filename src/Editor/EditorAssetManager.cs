using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core;

namespace UAlbion.Editor
{
    public class EditorAssetManager : ServiceComponent<IEditorAssetManager>, IEditorAssetManager
    {
        const int MaxUndo = 100;
        int _nextId;
        readonly Dictionary<int, EditorAsset> _assetsById = new Dictionary<int, EditorAsset>();
        readonly Dictionary<object, int> _idsByAsset = new Dictionary<object, int>();
        readonly LinkedList<IEditorEvent> _undoStack = new LinkedList<IEditorEvent>();

        public EditorAssetManager()
        {
            On<IEditorEvent>(Apply);
        }

        void Apply(IEditorEvent e)
        {
            var asset = GetAssetForId(e.Id);
            if (asset == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"No asset could be found with id {e.Id}"));
                return;
            }

            if (!asset.Apply(e))
                return;

            // TODO: Verify id is valid
            // TODO: Verify current value in event matches actual current value
            // TODO: Perform change

            _undoStack.AddLast(e);
            if (_undoStack.Count > MaxUndo)
                _undoStack.RemoveFirst();
        }

        void Undo(IEditorEvent e)
        {
            // Verify id is valid
            // Perform inverse of change
            throw new NotImplementedException();
        }

        public int GetIdForAsset(object asset)
        {
            if (_idsByAsset.TryGetValue(asset, out int id))
                return id;

            id = _nextId++;
            _assetsById[id] = new EditorAsset(asset);
            _idsByAsset[asset] = id;
            return id;
        }

        public EditorAsset GetAssetForId(int id) => _assetsById.TryGetValue(id, out var asset) ? asset : null;
    }
}