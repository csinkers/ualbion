using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public class SceneManager : Component, ISceneManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SceneManager, SetSceneEvent>((x, e) => x.Set(e.SceneId)),
            H<SceneManager, OpenCharacterInventoryEvent>((x, e) => x.OpenInventory(e.MemberId)),
            H<SceneManager, OpenChestEvent>((x,e) => x.OpenChest(e))
        );

        void OpenChest(OpenChestEvent e)
        {
            if(ActiveSceneId != SceneId.Inventory)
                Raise(new PushSceneEvent(SceneId.Inventory));

            // TODO: Handle messages, locked chests, traps etc
            var party = Resolve<IParty>();
            Raise(new InventoryChestModeEvent(e.ChestId, party.Leader));
        }

        void OpenInventory(PartyCharacterId memberId)
        {
            if(ActiveSceneId != SceneId.Inventory)
                Raise(new PushSceneEvent(SceneId.Inventory));
            Raise(new InventoryModeEvent(memberId));
        }

        void Set(SceneId activatingSceneId)
        {
            _scenes[ActiveSceneId].IsActive = false;
            foreach (var sceneId in _scenes.Keys)
            {
                var scene = _scenes[sceneId];
                scene.IsActive = sceneId == activatingSceneId;

                if (!scene.IsActive)
                    continue;

                var interfaces = scene.GetType().GetInterfaces();
                var sceneInterface = interfaces.FirstOrDefault(x => typeof(IScene).IsAssignableFrom(x) && x != typeof(IScene));
                if (sceneInterface != null)
                    Exchange.Register(sceneInterface, scene);
                else
                    Exchange.Attach(scene);
            }

            ActiveSceneId = activatingSceneId;
        }

        readonly IDictionary<SceneId, GameScene> _scenes = new Dictionary<SceneId, GameScene>();

        public SceneManager() : base(Handlers) { }
        public SceneId ActiveSceneId { get; private set; }
        public IScene GetScene(SceneId sceneId) => _scenes.TryGetValue(sceneId, out var scene) ? scene : null;

        public SceneManager AddScene(GameScene scene)
        {
            scene.IsActive = false;
            AttachChild(scene);
            _scenes.Add(scene.Id, scene);
            return this;
        }

        public IScene ActiveScene => _scenes[ActiveSceneId];
    }
}
