using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Combat;

public class VisualCombatTile : UiElement
{
    const int Width = 32;
    const int Height = 24;
    // const int SpriteHeight = 48;

    readonly int _tileIndex;
    readonly IReadOnlyBattle _battle;
    readonly UiSpriteElement _sprite;
    readonly Button _button;

    public SpriteId Icon
    {
        get => _sprite.Id;
        set
        {
            if (_sprite.Id == value) return;
            _sprite.Id = value;
            _sprite.IsActive = !value.IsNone;
        }
    }

    public VisualCombatTile(int tileIndex, IReadOnlyBattle battle)
    {
        On<PostEngineUpdateEvent>(_ => OnPostUpdate());
        _tileIndex = tileIndex;
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));
        _sprite = new UiSpriteElement(SpriteId.None) { IsActive = false, Flags = SpriteFlags.BottomAligned };

        var stack =
            new VerticalStacker(
                    new Spacing(Width, Height),
                    _sprite
            )
            {
                Greedy = false
            };

        _button = new Button(stack) { Margin = 0 }
            .OnHover(() => Hover?.Invoke())
            .OnBlur(() => Blur?.Invoke())
            .OnClick(() => Click?.Invoke())
            .OnRightClick(() => RightClick?.Invoke())
            .OnDoubleClick(() => DoubleClick?.Invoke())
            .OnButtonDown(() => ButtonDown?.Invoke());
        AttachChild(_button);
    }

    void OnPostUpdate()
    {
        IReadOnlyMob mob = _battle.GetTile(_tileIndex);
        Icon = mob == null ? SpriteId.None : mob.Sheet.TacticalGfx;
    }

    // public ButtonState State { get => _frame.State; set => _frame.State = value; }
    public override Vector2 GetSize() => new(Width, Height);
    public VisualCombatTile OnClick(Action callback) { Click += callback; return this; }
    public VisualCombatTile OnRightClick(Action callback) { RightClick += callback; return this; }
    public VisualCombatTile OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
    public VisualCombatTile OnButtonDown(Action callback) { ButtonDown += callback; return this; }
    public VisualCombatTile OnHover(Action callback) { Hover += callback; return this; }
    public VisualCombatTile OnBlur(Action callback) { Blur += callback; return this; }

    event Action Click;
    event Action DoubleClick;
    event Action RightClick;
    event Action ButtonDown;
    event Action Hover;
    event Action Blur;

    public bool Hoverable { get => _button.Hoverable; set => _button.Hoverable = value; }
    public bool SuppressNextDoubleClick { get => _button.SuppressNextDoubleClick; set => _button.SuppressNextDoubleClick = value; }

    // ReSharper disable once UnusedParameter.Local
    static void Rebuild(in Rectangle extents)
    {
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild(extents);
        return base.Render(extents, order, parent);
    }
}