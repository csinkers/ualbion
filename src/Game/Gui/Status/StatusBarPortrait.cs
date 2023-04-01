using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Status;

public class StatusBarPortrait : UiElement
{
    const string TimerName = "StatusBarPortrait.ClickTimer";

    readonly UiSpriteElement _portrait;
    readonly StatusBarHealthBar _health;
    readonly StatusBarHealthBar _mana;
    readonly int _order;
    bool _isClickTimerPending;

    public StatusBarPortrait(int order)
    {
        _order = order;
        _portrait = AttachChild(new UiSpriteElement(Base.Portrait.Tom));
        _health = AttachChild(new StatusBarHealthBar(order, true));
        _mana = AttachChild(new StatusBarHealthBar(order, false));

        On<PartyChangedEvent>(_ => LoadSprite());
        On<UiLeftClickEvent>(OnClick);
        On<UiRightClickEvent>(OnRightClick);
        On<HoverEvent>(Hover);
        On<BlurEvent>(_ =>
        {
            _portrait.Flags = 0;
            Raise(new HoverTextEvent(null));
        });
        On<TimerElapsedEvent>(e =>
        {
            if (e.Id == TimerName)
                OnTimer();
        });

    }

    void OnRightClick(UiRightClickEvent e)
    {
        var member = PartyMember;
        if (member == null)
            return;

        e.Propagating = false;
        var party = Resolve<IParty>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();
        var tf = Resolve<ITextFormatter>();

        var heading = new LiteralText(
            new TextBlock(member.Apparent.GetName(Var(UserVars.Gameplay.Language)))
            {
                Style = TextStyle.Fat,
                Alignment = TextAlignment.Center
            });

        IText S(TextId textId) => tf.Center().NoWrap().Format(textId);

        var uiPosition = window.PixelToUi(cursorManager.Position);
        var options = new List<ContextMenuOption>
        {
            new(
                S(Base.SystemText.PartyPopup_CharacterScreen),
                new InventoryOpenEvent(member.Id),
                ContextMenuGroup.Actions)
        };

        if (member.Apparent.Magic.SpellStrengths.Any())
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.PartyPopup_UseMagic),
                null,
                ContextMenuGroup.Actions));
        }

        if (member.Id != party.Leader.Id)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.PartyPopup_MakeLeader),
                new SetPartyLeaderEvent(member.Id, 3, 0), // TODO: what do unk2/3 do?
                ContextMenuGroup.Actions));
        }

        if (member.Id != Base.PartyMember.Tom)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.PartyPopup_TalkTo),
                new StartPartyDialogueEvent(member.Id), 
                ContextMenuGroup.Actions));
        }

        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }

    protected override void Subscribed() => LoadSprite();
    public override Vector2 GetSize() => _portrait.GetSize() + new Vector2(0,6); // Add room for health + mana bars
    IPlayer PartyMember
    {
        get
        {
            var ordered = TryResolve<IParty>()?.StatusBarOrder;
            return ordered == null || _order >= ordered.Count ? null : ordered[_order];
        }
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        var party = Resolve<IParty>();
        var member = PartyMember;
        if (member == null)
            return order;

        bool highlighted = member.Id == party.Leader.Id;
        int maxOrder = order;
        var portraitExtents = new Rectangle(extents.X, extents.Y + (highlighted ? 0 : 3), extents.Width, extents.Height - 6);

        var centreX = (portraitExtents.Left + portraitExtents.Right) / 2;
        var centreY = (portraitExtents.Top + portraitExtents.Bottom) / 2;
        if ((int)PartyMember.StatusBarUiPosition.X != centreX || 
            (int)PartyMember.StatusBarUiPosition.Y != centreY)
        {
            Raise(new SetPlayerStatusUiPositionEvent(PartyMember.Id, centreX, centreY));
        }

        maxOrder = Math.Max(maxOrder, func(_portrait, portraitExtents, order, context));
        maxOrder = Math.Max(maxOrder, func(_health, new Rectangle(
                extents.X + 5,
                extents.Y + extents.Height - 7,
                extents.Width - 12,
                4),
            order, context));

        if (member.Apparent.Magic.SpellPoints.Max > 0)
        {
            maxOrder = Math.Max(maxOrder, func(_mana, new Rectangle(
                    extents.X + 5,
                    extents.Y + extents.Height - 4,
                    extents.Width - 12,
                    4),
                order, context));
        }

        return maxOrder;
    }

    void LoadSprite()
    {
        var portraitId = PartyMember?.Apparent.PortraitId;
        _portrait.IsActive = portraitId.HasValue;
        if (portraitId.HasValue)
            _portrait.Id = portraitId.Value;
    }

    void OnClick(UiLeftClickEvent e)
    {
        e.Propagating = false;

        if (_isClickTimerPending) // If they double-clicked...
        {
            var memberId = PartyMember?.Id;
            if(memberId.HasValue)
                Raise(new InventoryOpenEvent(memberId.Value));
            _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
        }
        else // For the first click, just start the double-click timer.
        {
            Raise(new StartTimerEvent(TimerName, Var(GameVars.Ui.ButtonDoubleClickIntervalSeconds), this));
            _isClickTimerPending = true;
        }
    }

    void OnTimer()
    {
        if (!_isClickTimerPending) // They've already double-clicked
            return;

        _isClickTimerPending = false;

        var inventoryManager = Resolve<IInventoryManager>();
        if (inventoryManager?.ItemInHand.Item != null)
        {
            Raise(new InventoryGiveItemEvent(PartyMember.Id));
            return;
        }

        var memberId = PartyMember?.Id;
        if (memberId.HasValue)
            Raise(new SetPartyLeaderEvent(memberId.Value, 3, 0)); // TODO: Proper values for unk2/3
    }

    void Hover(HoverEvent e)
    {
        e.Propagating = false;
        _portrait.Flags = SpriteFlags.Highlight;
        var member = PartyMember;
        if (member == null)
            return;

        var inventoryManager = Resolve<IInventoryManager>();
        var tf = Resolve<ITextFormatter>();

        IText text;
        switch (inventoryManager.ItemInHand.Item.Type)
        {
            case AssetType.Item:
            {
                // Give %s to %s
                var item = Resolve<IAssetManager>().LoadItem(inventoryManager.ItemInHand.Item);
                text = tf.Format(
                    Base.SystemText.PartyPortrait_GiveXToX,
                    item.Name,
                    member.Apparent.GetName(Var(UserVars.Gameplay.Language)));
                break;
            }
            case AssetType.Gold:
                // Give gold to %s
                text = tf.Format(
                    Base.SystemText.PartyPortrait_GiveGoldToX,
                    member.Apparent.GetName(Var(UserVars.Gameplay.Language)));
                break;
            case AssetType.Rations:
                // Give food to %s
                text = tf.Format(
                    Base.SystemText.PartyPortrait_GiveFoodToX,
                    member.Apparent.GetName(Var(UserVars.Gameplay.Language)));
                break;
            default:
                // %s (LP:%d, SP:%d)
                text = tf.Format(
                    Base.SystemText.PartyPortrait_XLifeMana,
                    member.Apparent.GetName(Var(UserVars.Gameplay.Language)),
                    member.Apparent.Combat.LifePoints.Current,
                    member.Apparent.Magic.SpellPoints.Current);
                break;
        }

        Raise(new HoverTextEvent(text));
    }
}