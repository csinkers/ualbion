using System;

namespace UAlbion.Game.Scenes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SceneAttribute : Attribute
{
    public SceneAttribute(SceneId sceneId) => SceneId = sceneId;
    public SceneId SceneId { get; }
}