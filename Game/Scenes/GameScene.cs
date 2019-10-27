using System;
using System.Collections.Generic;
using UAlbion.Core;

namespace UAlbion.Game.Scenes
{
    public class GameScene : Scene
    {
        public SceneId Id { get; }

        protected GameScene(SceneId sceneId, ICamera camera, IList<Type> activeActiveRendererTypes) 
            : base(sceneId.ToString(), camera, activeActiveRendererTypes)
        {
            Id = sceneId;
        }
    }
    // Interfaces just for resolving specific scenes in dependent components
}