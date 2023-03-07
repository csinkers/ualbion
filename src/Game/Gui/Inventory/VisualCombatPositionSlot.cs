using System;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory;

public sealed class VisualCombatPositionSlot : UiElement
{
    readonly UiSpriteElement _sprite;
    readonly int _slotNumber;
    readonly Func<int, ICharacterSheet> _getSheet;
    readonly Button _button;

    public VisualCombatPositionSlot(int slotNumber, Func<int, ICharacterSheet> getSheet)
    {
        _slotNumber = slotNumber;
        _getSheet = getSheet ?? throw new ArgumentNullException(nameof(getSheet));

        _sprite = new UiSpriteElement(SpriteId.None);
        _button = AttachChild(new Button(
                new FixedPositionStacker().Add(_sprite, 0, 0, 36, 38))
            {
                Padding = -1,
                Margin = 0,
                Theme = ButtonTheme.InventorySlot
            }
            .OnHover(() => Hover?.Invoke())
            .OnBlur(() => Blur?.Invoke())
            .OnClick(() => Click?.Invoke()));
    }

    public VisualCombatPositionSlot OnClick(Action callback) { Click += callback; return this; }
    public VisualCombatPositionSlot OnHover(Action callback) { Hover += callback; return this; } 
    public VisualCombatPositionSlot OnBlur(Action callback) { Blur += callback; return this; } 
    public bool Hoverable { get => _button.Hoverable; set => _button.Hoverable = value; }

    event Action Click;
    event Action Hover;
    event Action Blur;

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        _sprite.Id = _getSheet(_slotNumber)?.PortraitId ?? SpriteId.None;
        return base.Render(extents, order, parent);
    }
}