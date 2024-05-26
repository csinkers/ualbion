using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State;

public class SceneManager : Container, ISceneManager
{
    readonly Dictionary<SceneId, IScene> _scenes = new();

    public SceneManager() : base("SceneManager")
    {
        On<SetSceneEvent>(Set);
    }

    public SceneId ActiveSceneId { get; private set; }
    public IScene GetScene(SceneId sceneId) => _scenes.GetValueOrDefault(sceneId);
    public IScene ActiveScene => _scenes[ActiveSceneId];
    protected override void Subscribing()
    {
        Exchange.Register(typeof(ISceneManager), this, false);
        Exchange.Register(typeof(ICameraProvider), this, false);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(ISceneManager), this);
        Exchange.Unregister(typeof(ICameraProvider), this);
    }

    protected override bool AddingChild(IComponent child)
    {
        if (child is not IScene scene)
            return true;

        var attrib = (SceneAttribute)scene.GetType().GetCustomAttribute(typeof(SceneAttribute));
        if (attrib == null)
            throw new InvalidOperationException($"Expected a Scene attribute on type {scene.GetType().Name}");

        scene.IsActive = false;
        _scenes.Add(attrib.SceneId, scene);
        return true;
    }

    void Set(SetSceneEvent e)
    {
        if (e.SceneId == ActiveSceneId)
            return;

        foreach (var kvp in _scenes)
        {
            if (kvp.Key == e.SceneId) continue; 
            kvp.Value.IsActive = false;
        }

        var newScene = _scenes[e.SceneId];
        newScene.IsActive = true;
        Exchange.Attach(newScene);

        var interfaces = newScene.GetType().GetInterfaces();
        var sceneInterface = interfaces.FirstOrDefault(x => typeof(IScene).IsAssignableFrom(x) && x != typeof(IScene));
        if (sceneInterface != null)
            Exchange.Register(sceneInterface, newScene);

        ActiveSceneId = e.SceneId;
    }

    public ICamera Camera => ActiveScene.Camera;
}