using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public interface ISceneManager
    {
        EventExchange GetExchange(SceneId id);
        IScene ActiveScene { get; }
        SceneId ActiveSceneId { get; }
    }

    public class SceneManager : Component, ISceneManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SceneManager, SetSceneEvent>((x, e) => x.Set(e.SceneId)),
            H<SceneManager, OpenCharacterInventoryEvent>((x,e) => x.OpenInventory(e.MemberId))
        );

        void OpenInventory(PartyCharacterId memberId)
        {
            if(ActiveSceneId != SceneId.Inventory)
                Raise(new PushSceneEvent(SceneId.Inventory));
            Raise(new SetInventoryModeEvent(memberId));
        }

        void Set(SceneId activatingSceneId)
        {
            foreach (var sceneId in _scenes.Keys.Where(x => x != activatingSceneId).ToList())
            {
                var sceneExchange = GetExchange(sceneId);
                var scene = _scenes[sceneId];
                scene.Detach();
                sceneExchange.IsActive = false;
            }

            foreach (var sceneId in _scenes.Keys.Where(x => x == activatingSceneId).ToList())
            {
                var sceneExchange = GetExchange(sceneId);
                var scene = _scenes[sceneId];
                var interfaces = scene.GetType().GetInterfaces();
                var sceneInterface = interfaces.FirstOrDefault(x => typeof(IScene).IsAssignableFrom(x) && x != typeof(IScene));
                if (sceneInterface != null)
                    Exchange.Register(sceneInterface, scene);
                else
                    Exchange.Attach(scene);
                sceneExchange.IsActive = true;
            }

            ActiveSceneId = activatingSceneId;
        }

        readonly IDictionary<SceneId, GameScene> _scenes = new Dictionary<SceneId, GameScene>();
        readonly IDictionary<SceneId, EventExchange> _exchanges = new Dictionary<SceneId, EventExchange>();

        public SceneManager() : base(Handlers) { }
        public SceneId ActiveSceneId { get; private set; }

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

        public IScene ActiveScene => _scenes[ActiveSceneId];
    }
}