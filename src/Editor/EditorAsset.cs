using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Editor;

public class EditorAsset : Component
{
    readonly object _asset;

    public EditorAsset(object asset)
    {
        _asset = asset ?? throw new ArgumentNullException(nameof(asset));
    }

    public bool IsDirty { get; private set; }

    public object GetProperty(string name)
    {
        var property = _asset.GetType().GetProperty(name);
        if(property == null)
            throw new InvalidOperationException($"Tried to read invalid property \"{name}\" on object \"{_asset}\" of type \"{_asset.GetType()}\"");

        return property.GetValue(_asset);
    }

    public void SetProperty(string name, object value)
    {
        var property = _asset.GetType().GetProperty(name);
        if(property == null)
            throw new InvalidOperationException($"Tried to write to invalid property \"{name}\" on object \"{_asset}\" of type \"{_asset.GetType()}\"");

        property.SetValue(_asset, value);
        IsDirty = true;
    }

    public void AddToCollection(string collectionName, int index)
    {
        var assetType = _asset.GetType();
        var property = assetType.GetProperty(collectionName);
        if(property == null)
            throw new InvalidOperationException($"Tried to read invalid property \"{collectionName}\" on object \"{_asset}\" of type \"{_asset.GetType()}\"");

        var collection = property.GetValue(_asset) as IList;
        var collectionType = property.PropertyType;
        foreach(var interfaceType in collectionType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                Type itemType = interfaceType.GetGenericArguments()[0];
                var constructor = itemType.GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);
                if (constructor == null)
                    throw new InvalidOperationException($"Tried to add a new instance of type {itemType} to the collection {collectionName} on {_asset} of type {assetType} which does not have a parameterless public constructor");
                var newInstance = constructor.Invoke(Array.Empty<object>());

                if(collection == null)
                    throw new InvalidOperationException($"Tried to add a new instance of type {itemType} to the collection {collectionName} on {_asset} of type {assetType}, but the collection is null or of non-IList type");
                collection.Insert(index, newInstance);
                IsDirty = true;
            }
        }
    }

    public void RemoveFromCollection(string collectionName, int index)
    {
        throw new NotImplementedException();
    }

    public void MoveInCollection(string collectionName, int fromIndex, int toIndex)
    {
        throw new NotImplementedException();
    }

    public bool Apply(IEditorEvent editorEvent)
    {
        if (editorEvent == null) throw new ArgumentNullException(nameof(editorEvent));
        switch (editorEvent)
        {
            case EditorSetPropertyEvent setProperty:
                var currentValue = GetProperty(setProperty.PropertyName);
                if (!Equals(currentValue, setProperty.CurrentValue))
                {
                    Error($"Tried to change {_asset.GetType().Name}.{setProperty.PropertyName} " +
                          $"from {setProperty.CurrentValue} to {setProperty.NewValue}, but it is currently set to {currentValue}");
                    return false;
                }

                SetProperty(setProperty.PropertyName, setProperty.NewValue);
                return true;

            case EditorAddInstanceEvent addInstance:
                // TODO: Validation
                AddToCollection(addInstance.CollectionName, addInstance.Index);
                return true;

            case EditorRemoveInstanceEvent removeInstance:
                // TODO: Validation
                RemoveFromCollection(removeInstance.CollectionName, removeInstance.Index);
                return true;

            case EditorMoveInstanceEvent moveInstance:
                MoveInCollection(moveInstance.CollectionName, moveInstance.FromIndex, moveInstance.ToIndex);
                return true;

            default:
                Error($"Could not handle IEditorEvent of type {editorEvent.GetType()}");
                return false;
        }
    }
}