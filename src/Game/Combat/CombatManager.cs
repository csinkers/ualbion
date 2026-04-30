using UAlbion.Api.Eventing;
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

        // DEVIATION: original always had a background in encounter events; fall back to Dungeon (pal.24) if none
        if (backgroundId.IsNone)
            backgroundId = new CombatBackgroundId(2); // CombatBackground.Dungeon

        Raise(new PushSceneEvent(SceneId.Combat));

        var info = Assets.GetAssetInfo(backgroundId);
        if (info != null)
            Raise(new LoadPaletteEvent(info.PaletteId));

        var scene = Resolve<ISceneManager>().ActiveScene;
        var battle = new Battle(groupId, backgroundId);
        scene.Add(battle);

        Raise(new DialogManager.CombatDialogEvent(battle));

        // Source: Horneman COMBAT.C Enter_Combat switch on Combat_status
        battle.Complete += result =>
        {
            Raise(new LogEvent(LogLevel.Info, $"[COMBAT MANAGER] battle.Complete fired result={result}"));
            scene.Remove(battle);
            switch (result)
            {
                case CombatResult.Victory:
                    // Apres dialog already shown by Battle; just return to map.
                    Raise(new PopSceneEvent());
                    break;
                case CombatResult.PartyKilled:
                    // DEVIATION: no dedicated Game_over screen yet — push main menu as fallback.
                    Raise(new PopSceneEvent());
                    Raise(new PushSceneEvent(SceneId.MainMenu));
                    break;
                default: // Retreat / Ended — return to map directly.
                    Raise(new PopSceneEvent());
                    break;
            }
        };
    }
}
