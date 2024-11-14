using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;

namespace UAlbion.Editor;

public class EditorAssetManager : ServiceComponent<IEditorAssetManager>, IEditorAssetManager
{
    const int MaxUndo = 100;
    int _nextId;
    readonly Dictionary<int, EditorAsset> _assetsById = [];
    readonly Dictionary<object, int> _idsByAsset = [];
    readonly LinkedList<IEditorEvent> _undoStack = new();
    // Cross references: IDictionary<AssetId, IList<(object, string)>> - requires loading total state.

    public EditorAssetManager()
    {
        On<EditorAddInstanceEvent>(Apply);
        On<EditorAggregateChangeEvent>(Apply);
        On<EditorMoveInstanceEvent>(Apply);
        On<EditorRemoveInstanceEvent>(Apply);
        On<EditorSetPropertyEvent>(Apply);
    }

    void Apply(IEditorEvent e)
    {
        if (e is EditorAggregateChangeEvent aggregate)
        {
            var applied = new List<IEditorEvent>();
            bool success = true;
            foreach (var subEvent in aggregate.Events)
            {
                success &= InnerApply(subEvent);
                if (success)
                    applied.Add(subEvent);
                else break;
            }

            if (!success)
            {
                applied.Reverse();
                foreach (var subEvent in applied)
                    Undo(subEvent);
                return;
            }
        }
        else
        {
            if (!InnerApply(e))
                return;
        }

        _undoStack.AddLast(e);
        if (_undoStack.Count > MaxUndo)
            _undoStack.RemoveFirst();
    }

    bool InnerApply(IEditorEvent e)
    {
        var asset = GetAssetForId(e.Id);
        if (asset != null)
            return asset.Apply(e);

        Error($"No asset could be found with id {e.Id}");
        return false;
    }

    void Undo(IEditorEvent _)
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

    public EditorAsset GetAssetForId(int id) => _assetsById.GetValueOrDefault(id);
}