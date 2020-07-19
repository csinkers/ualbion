using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.Text;

namespace UAlbion.Game.State
{
    public class SceneManager : ServiceComponent<ISceneManager>, ISceneManager
    {
        readonly IDictionary<SceneId, GameScene> _scenes = new Dictionary<SceneId, GameScene>();

        public SceneManager()
        {
            On<SetSceneEvent>(Set);
            On<ChestEvent>(OpenChest);
            On<DoorEvent>(OpenDoor);
            On<ISetInventoryModeEvent>(e => OpenInventory(e.Member));
            On<InventoryOpenPositionEvent>(e =>
            {
                var party = Resolve<IParty>();
                if (party?.StatusBarOrder.Count > e.Position)
                    OpenInventory(party.StatusBarOrder[e.Position].Id);
            });
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

        void OpenChest(ChestEvent e)
        {
            if(ActiveSceneId != SceneId.Inventory)
                Raise(new PushSceneEvent(SceneId.Inventory));

            // TODO: Handle messages, locked chests, traps etc
            var party = Resolve<IParty>();
            Raise(e.Member.HasValue ? e : e.CloneForMember(party.Leader));
        }

        void OpenDoor(DoorEvent obj)
        {
        }

        void OpenInventory(PartyCharacterId? memberId)
        {
            if(ActiveSceneId != SceneId.Inventory)
                Raise(new PushSceneEvent(SceneId.Inventory));

            var party = Resolve<IParty>();
            memberId ??= party.Leader;
            Raise(new SetContextEvent(ContextType.Inventory, AssetType.PartyMember, (int)memberId));
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
            if(ActiveSceneId != SceneId.Inventory)
            {
                // _inventoryMode = InventoryMode.Character;
                // _inventoryId = null;
            }
        }
    }
}
