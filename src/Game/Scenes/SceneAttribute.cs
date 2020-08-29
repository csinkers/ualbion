using System;

namespace UAlbion.Game.Scenes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SceneAttribute : Attribute
    {
        public SceneAttribute(SceneId id) => SceneId = id;
        public SceneId SceneId { get; }
    }
}
