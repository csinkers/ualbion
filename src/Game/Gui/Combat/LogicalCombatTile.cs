using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Combat;

public class LogicalCombatTile : UiElement
{
    class NopEvent : Event { }

    readonly int _tileIndex;
    readonly IReadOnlyBattle _battle;

    public LogicalCombatTile(int tileIndex, IReadOnlyBattle battle)
    {
        _tileIndex = tileIndex;
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));

        AttachChild(new VisualCombatTile(tileIndex, battle))
            .OnClick(OnClick)
            .OnRightClick(OnRightClick)
            .OnHover(Hover)
            .OnBlur(Blur);

        On<CombatDamageFloaterEvent>(e =>
        {
            if (e.Tile == _tileIndex)
                AttachChild(new DamageFloater(e.Damage));
        });
    }

    public override string ToString() => $"CombatTile:{_tileIndex}";

    void OnClick()
    {
        // During target selection, any tile click completes the pending action.
        if (_battle.PlanningState == CombatPlanningState.SelectingTarget)
            Raise(new SelectCombatTargetEvent(_tileIndex));
    }

    void Blur()
    {
        Raise(new HoverTextEvent(null));
    }

    void Hover()
    {
        var contents = _battle.GetTile(_tileIndex);
        var sheet = contents?.Effective;
        if (sheet != null)
        {
            var name = sheet.GetName(ReadVar(V.User.Gameplay.Language));
            var lp = sheet.Combat.LifePoints.Current;
            var maxLp = sheet.Combat.LifePoints.Max;
            var sp = sheet.Magic.SpellPoints.Current;
            var maxSp = sheet.Magic.SpellPoints.Max;
            Raise(new HoverTextEvent(new LiteralText($"{name} (LP:{lp}/{maxLp}, SP:{sp}/{maxSp})")));
        }
    }

    bool IsMagicItem(IReadOnlyItemSlot slot)
    {
        if (slot.Item.IsNone || slot.Item.Type != AssetType.Item)
            return false;

        var item = Assets.LoadItem(slot.Item);
        if (item == null)
            return false;

        return !item.Spell.IsNone;
    }

    void OnRightClick()
    {
        var contents = _battle.GetTile(_tileIndex);
        var sheet = contents?.Effective;

        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();

        var playerName = sheet?.GetName(ReadVar(V.User.Gameplay.Language));
        IText heading = playerName == null
            ? tf.Center().NoWrap().Fat().Format(Base.SystemText.Combat_Combat)
            : tf.Center().NoWrap().Fat().Format(playerName);

        IText S(TextId textId, bool disabled = false)
            => tf
                .Center()
                .NoWrap()
                .Ink(disabled ? Base.Ink.Yellow : Base.Ink.White)
                .Format(textId);

        var options = new List<ContextMenuOption>();

        if (sheet?.Type == CharacterType.Party)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.Combat_DoNothing),
                new SelectCombatActionEvent(_tileIndex, CombatActionType.None),
                ContextMenuGroup.Actions));

            options.Add(new ContextMenuOption(
                S(Base.SystemText.Combat_Attack, sheet.DisplayDamage <= 0
                    && sheet.Combat.BaseAttack == 0 && sheet.Combat.BonusAttack <= 0),
                new SelectCombatActionEvent(_tileIndex, CombatActionType.Attack),
                ContextMenuGroup.Actions));

            options.Add(new ContextMenuOption(
                S(Base.SystemText.Combat_Move),
                new SelectCombatActionEvent(_tileIndex, CombatActionType.Move),
                ContextMenuGroup.Actions));

            if (sheet.Magic.KnownSpells.Count > 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.Combat_UseMagic),
                    new SelectCombatActionEvent(_tileIndex, CombatActionType.CastSpell),
                    ContextMenuGroup.Actions));
            }

            if (sheet.Inventory.EnumerateAll().Any(IsMagicItem))
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.Combat_UseMagicItem),
                    new SelectCombatActionEvent(_tileIndex, CombatActionType.UseMagicItem),
                    ContextMenuGroup.Actions));
            }

            if (_tileIndex / SavedGame.CombatColumns == 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.Combat_Flee),
                    new SelectCombatActionEvent(_tileIndex, CombatActionType.Flee),
                    ContextMenuGroup.Actions));
            }
        }

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_AdvanceParty),
            new NopEvent(),
            ContextMenuGroup.Actions2));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_Observe),
            new ObserveCombatEvent(),
            ContextMenuGroup.System));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.MapPopup_MainMenu),
            new PushSceneEvent(SceneId.MainMenu),
            ContextMenuGroup.System));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_EndCombat),
            new EndCombatEvent(CombatResult.Victory),
            ContextMenuGroup.System));

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }
}
