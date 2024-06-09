using System;
using System.Collections.Generic;
using System.Reflection;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public class ResourceLayoutSource : ServiceComponent<IResourceLayoutSource>, IResourceLayoutSource
{
    readonly object _syncRoot = new();
    readonly Dictionary<Type, ResourceLayout> _layouts = new();

    public ResourceLayoutSource()
    {
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Unsubscribed() => Dispose();

    public ResourceLayout GetLayout(Type type, GraphicsDevice device)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(device);

        lock (_syncRoot)
        {
            if (_layouts.TryGetValue(type, out var layout))
                return layout;

            var field = type.GetField("Layout", BindingFlags.Static | BindingFlags.Public);
            if (field == null)
                throw new InvalidOperationException($"Tried to retrieve resource layout from type {type.Name}, but it does not contain a public static \"Layout\" field");

            if(field.FieldType != typeof(ResourceLayoutDescription))
                throw new InvalidOperationException($"Tried to retrieve resource layout from type {type.Name}, but its \"Layout\" field is not of type ResourceLayoutDescription");

            var description = (ResourceLayoutDescription)field.GetValue(null)!;
            layout = device.ResourceFactory.CreateResourceLayout(ref description);
            _layouts[type] = layout;
            return layout;
        }
    }

    void Dispose()
    {
        lock (_syncRoot)
        {
            foreach(var kvp in _layouts)
                kvp.Value.Dispose();
            _layouts.Clear();
        }
    }
}