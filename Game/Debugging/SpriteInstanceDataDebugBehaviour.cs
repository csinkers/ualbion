using System;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Debugging
{
    public class SpriteInstanceDataDebugBehaviour : IDebugBehaviour
    {
        public Type HandledType => typeof(SpriteInstanceData);
        public void Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected)
        {
            if (reflected?.Parent == null) return;

            if (!(reflected.Parent.Object is SpriteInstanceData[] array))
                return;

            switch (action)
            {
                case DebugInspectorAction.Hover: array[reflected.CollectionIndex].Flags |= SpriteFlags.RedTint; break;
                case DebugInspectorAction.Blur: array[reflected.CollectionIndex].Flags &= ~SpriteFlags.RedTint; break;
            }
        }
    }
}