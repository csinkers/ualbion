﻿using System;
using System.Collections.ObjectModel;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Debugging
{
    public class SpriteInstanceDataDebugBehaviour : IDebugBehaviour
    {
        public ReadOnlyCollection<Type> HandledTypes { get; } = new(new[] { typeof(SpriteInstanceData) });
        public object Handle(DebugInspectorAction action, ReflectedObject reflected, EventExchange exchange)
        {
            if (reflected?.Parent?.Target is not SpriteInstanceData[] array)
                return null;

            switch (action)
            {
                case DebugInspectorAction.Hover: array[reflected.CollectionIndex].Flags |= SpriteFlags.RedTint; break;
                case DebugInspectorAction.Blur: array[reflected.CollectionIndex].Flags &= ~SpriteFlags.RedTint; break;
            }

            return null;
        }
    }
}
