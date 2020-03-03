using System;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Debugging
{
    public class SpriteInstanceDataDebugBehaviour : IDebugBehaviour
    {
        public Type[] HandledTypes { get; } = { typeof(SpriteInstanceData) };
        public object Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected)
        {
            if (reflected?.Parent == null) return null;

            if (!(reflected.Parent.Object is SpriteInstanceData[] array))
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