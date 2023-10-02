using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Combat;

public class VisualCombatTile : UiElement
{
    readonly int _tileIndex;
    readonly IReadOnlyBattle _battle;
    const int Width = 32;
#pragma warning disable CA1823 // Avoid unused private fields
    const int Height = 48;
#pragma warning restore CA1823 // Avoid unused private fields
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
        _tileIndex = tileIndex;
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));
        _sprite = new UiSpriteElement(SpriteId.None) { IsActive = false };

        _button = AttachChild(new Button(
                new VerticalStacker(
                    new Spacing(Width, Width),
                        _sprite)
                    { Greedy = false })
            .OnHover(() => Hover?.Invoke())
            .OnBlur(() => Blur?.Invoke())
            .OnClick(() => Click?.Invoke())
            .OnRightClick(() => RightClick?.Invoke())
            .OnDoubleClick(() => DoubleClick?.Invoke())
            .OnButtonDown(() => ButtonDown?.Invoke()));
    }

    // public ButtonState State { get => _frame.State; set => _frame.State = value; }
    public override Vector2 GetSize() => new(Width);
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

    static void Rebuild(in Rectangle extents)
    {
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild(extents);
        return base.Render(extents, order, parent);
    }
}