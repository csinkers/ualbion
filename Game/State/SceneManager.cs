using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public interface ISceneManager
    {
        EventExchange GetExchange(SceneId id);
    }

    public class SceneManager : Component, ISceneManager
    {
        readonly Engine _engine;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SceneManager, SetSceneEvent>((x, e) => x.Set(e.SceneId))
        );

        void Set(SceneId activatingSceneId)
        {
            foreach (var sceneId in _scenes.Keys.ToList())
            {
                var sceneExchange = GetExchange(sceneId);
                var scene = _scenes[sceneId];
                if (activatingSceneId == sceneId)
                {
                    var interfaces = scene.GetType().GetInterfaces();
                    var sceneInterface = interfaces.FirstOrDefault(x => typeof(IScene).IsAssignableFrom(x) && x != typeof(IScene));
                    if (sceneInterface != null)
                        Exchange.Register(sceneInterface, scene);
                    else
                        Exchange.Attach(scene);
                    sceneExchange.IsActive = true;
                }
                else
                {
                    scene.Detach();
                    sceneExchange.IsActive = false;
                }
            }
        }

        readonly IDictionary<SceneId, GameScene> _scenes = new Dictionary<SceneId, GameScene>();
        readonly IDictionary<SceneId, EventExchange> _exchanges = new Dictionary<SceneId, EventExchange>();

        public SceneManager(Engine engine) : base(Handlers) { _engine = engine; }

        public SceneManager AddScene(GameScene scene)
        {
            _scenes.Add(scene.Id, scene);
            return this;
        }

        public EventExchange GetExchange(SceneId id)
        {
            if(!_exchanges.ContainsKey(id))
                _exchanges[id] = new EventExchange($"Scene:{id}", Exchange);
            return _exchanges[id];
        }
    }
}