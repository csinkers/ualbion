using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.Combat;

public class CombatManager : Component
{
    public CombatManager()
    {
        On<EncounterEvent>(e => BeginCombat(e.GroupId, e.BackgroundId));
    }

    void BeginCombat(MonsterGroupId groupId, SpriteId backgroundId)
    {
        if (backgroundId.IsNone)
            backgroundId = Resolve<IMapManager>().Current.MapData.CombatBackgroundId;

        var paletteId = PaletteId.FromUInt32(Resolve<IPaletteManager>().Day.Id);
        Raise(new PushSceneEvent(SceneId.Combat));
        Raise(new LoadPaletteEvent(paletteId));

        var battle = AttachChild(new Battle(groupId, backgroundId));
        Raise(new DialogManager.ShowCombatDialogEvent(battle));

        battle.Complete += () =>
        {
            RemoveChild(battle);
            Raise(new PopSceneEvent());
        };
    }
}