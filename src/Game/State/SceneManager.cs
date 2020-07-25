using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public class SceneManager : ServiceComponent<ISceneManager>, ISceneManager
    {
        readonly IDictionary<SceneId, GameScene> _scenes = new Dictionary<SceneId, GameScene>();

        public SceneManager()
        {
            On<SetSceneEvent>(Set);
        }

        public SceneId ActiveSceneId { get; private set; }
        public IScene GetScene(SceneId sceneId) => _scenes.TryGetValue(sceneId, out var scene) ? scene : null;
        public IScene ActiveScene => _scenes[ActiveSceneId];

        public SceneManager AddScene(GameScene scene)
        {
            scene.IsActive = false;
            AttachChild(scene);
            _scenes.Add(scene.Id, scene);
            return this;
        }

        void Set(SetSceneEvent e)
        {
            _scenes[ActiveSceneId].IsActive = false;
            foreach (var sceneId in _scenes.Keys)
            {
                var scene = _scenes[sceneId];
                scene.IsActive = sceneId == e.SceneId;

                if (!scene.IsActive)
                    continue;

                var interfaces = scene.GetType().GetInterfaces();
                var sceneInterface = interfaces.FirstOrDefault(x => typeof(IScene).IsAssignableFrom(x) && x != typeof(IScene));
                if (sceneInterface != null)
                    Exchange.Register(sceneInterface, scene);
                else
                    Exchange.Attach(scene);
            }

            ActiveSceneId = e.SceneId;
        }
    }
}
