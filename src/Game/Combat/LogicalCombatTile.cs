using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.Text;

namespace UAlbion.Game.Combat;

public class LogicalCombatTile : UiElement
{
    readonly int _tileIndex;
    readonly VisualCombatTile _visual;

    public LogicalCombatTile(int tileIndex)
    {
        _tileIndex = tileIndex;

        _visual = AttachChild(new VisualCombatTile())
            .OnClick(() =>
            {
            })
            .OnRightClick(OnRightClick)
            .OnHover(Hover)
            .OnBlur(Blur);
    }

    public override string ToString() => $"CombatTile:{_tileIndex}";

    void Blur()
    {
        // Raise(new SetCursorEvent(hand.Item.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
        // Raise(new HoverTextEvent(null));
    }

    void Hover()
    {
        // _visual.Hoverable = true;
    }

    void OnRightClick()
    {
        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();

        var heading = tf.Center().NoWrap().Fat().Format("TODO");

        IText S(TextId textId, bool disabled = false)
            => tf
                .Center()
                .NoWrap()
                .Ink(disabled ? Base.Ink.Yellow : Base.Ink.White)
                .Format(textId);

        // Drop (Yellow inactive when critical)
        // Examine
        // Use (e.g. torch)
        // Drink
        // Activate (compass, clock, monster eye)
        // Activate spell (if has spell, yellow if combat spell & not in combat etc)
        // Read (e.g. metal-magic knowledge, maps)

        var options = new List<ContextMenuOption>();

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_DoNothing),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_Attack),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_Move),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_UseMagic),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_UseMagicItem),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_Flee),
            new NopEvent(),
            ContextMenuGroup.Actions));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_AdvanceParty),
            new NopEvent(),
            ContextMenuGroup.Actions2));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.Combat_Observe),
            new NopEvent(),
            ContextMenuGroup.System));

        options.Add(new ContextMenuOption(
            S(Base.SystemText.MapPopup_MainMenu),
            new NopEvent(),
            ContextMenuGroup.System));

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }
}

class NopEvent : Event { }