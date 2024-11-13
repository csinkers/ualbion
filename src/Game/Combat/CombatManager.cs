using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion.Game.Combat;

public class CombatManager : GameComponent
{
    /*
    CombatManager
    |--Battle
      |--List<Mob> _mobs
      |--Mob[] _tiles
      |--Sprite (background)
      |-- Mobs contain a Monster, created by the Battle
    |--CombatDialog (owned by DialogManager, but CombatManager asks for it to be created)


     */
    public CombatManager()
    {
        On<EncounterEvent>(e => BeginCombat(e.GroupId, e.BackgroundId));
    }

    void BeginCombat(MonsterGroupId groupId, SpriteId backgroundId)
    {
        if (backgroundId.IsNone)
            backgroundId = Resolve<IMapManager>().Current.MapData.CombatBackgroundId;

        Raise(new PushSceneEvent(SceneId.Combat));

        var info = Assets.GetAssetInfo(backgroundId);
        if (info != null)
            Raise(new LoadPaletteEvent(info.PaletteId));

        var scene = Resolve<ISceneManager>().ActiveScene;
        var battle = new Battle(groupId, backgroundId);
        scene.Add(battle);

        Raise(new DialogManager.CombatDialogEvent(battle));

        battle.Complete += () =>
        {
            scene.Remove(battle);
            Raise(new PopSceneEvent());
        };
    }
}
