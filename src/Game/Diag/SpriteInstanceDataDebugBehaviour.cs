using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Diag;

public class SpriteInstanceDataDebugBehaviour : Component, IDebugBehaviour
{
    public ReadOnlyCollection<Type> HandledTypes { get; } = new([typeof(SpriteInfo)]);
    /*public object Handle(DebugInspectorAction action, in ReflectorState state)
    {
        if (state.Parent is not SpriteInfo[] array)
            return null;

        switch (action)
        {
            case DebugInspectorAction.Hover: array[state.CollectionIndex].Flags |= SpriteFlags.RedTint; break;
            case DebugInspectorAction.Blur: array[state.CollectionIndex].Flags &= ~SpriteFlags.RedTint; break;
        }

        return null;
    }*/
}