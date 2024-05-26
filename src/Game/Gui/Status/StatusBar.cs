using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Status;

public class StatusBar : Dialog
{
    const int MaxPortraits = SavedGame.MaxPartySize;
    readonly UiSpriteElement _sprite;
    readonly StatusBarPortrait[] _portraits;
    readonly TextFilter _hoverSource = new(x => { x.Alignment = TextAlignment.Center; return true; });
    readonly TextFilter _descriptionSource = new(x => { x.Alignment = TextAlignment.Center; return true; });
    readonly FixedPosition _hoverTextContainer;
    readonly FixedPosition _descriptionTextContainer;

    public StatusBar() : base(DialogPositioning.StatusBar)
    {
        On<HoverTextEvent>(e => _hoverSource.Source = e.Source);
        On<DescriptionTextEvent>(e => _descriptionSource.Source = e.Source);

        _sprite = AttachChild(new UiSpriteElement(Base.UiBackground.Slab));
        _sprite.SubId = 1;
        _portraits = new StatusBarPortrait[MaxPortraits];
        for (int i = 0; i < _portraits.Length; i++)
        {
            _portraits[i] = new StatusBarPortrait(i);
            AttachChild(_portraits[i]);
        }

        var hoverText = new UiText(_hoverSource);
        var descriptionText = new UiText(_descriptionSource);
        _hoverTextContainer = AttachChild(new FixedPosition(new Rectangle(181, 196, 177, 10), hoverText));
        _descriptionTextContainer = AttachChild(new FixedPosition(new Rectangle(181, 208, 177, 30), descriptionText));
    }

    public override Vector2 GetSize() => new(UiConstants.StatusBarExtents.Width, UiConstants.StatusBarExtents.Height);

    int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func, bool trimOverlap)
    {
        int maxOrder = order;
        maxOrder = Math.Max(maxOrder, func(_sprite, extents, order + 1, context));
        maxOrder = Math.Max(maxOrder, func(_hoverTextContainer, extents, order + 2, context));
        maxOrder = Math.Max(maxOrder, func(_descriptionTextContainer, extents, order + 2, context));

        var party = TryResolve<IParty>();
        if (party == null)
            return maxOrder;

        for (int i = 0; i < _portraits.Length; i++)
        {
            if (i >= party.StatusBarOrder.Count)
                break;

            var portrait = _portraits[i];
            var member = party.StatusBarOrder[i];
            var portraitExtents = new Rectangle(
                extents.X + 4 + 28 * i,
                extents.Y + 3,
                (int)portrait.GetSize().X - (trimOverlap ? 6 : 0),
                (int)portrait.GetSize().Y);

            // Make sure to break any ties between the images so
            // there's a consistent order.
            int orderBias = 2 + i;
            if (party.Leader == member)
                orderBias = 2 + SavedGame.MaxPartySize;

            maxOrder = Math.Max(maxOrder, func(portrait, portraitExtents, order + orderBias, context));
        }

        return maxOrder;
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
            return order;

        int maxOrder = DoLayout(extents, order, context, SelectChild, true);
        context.AddHit(order, this);
        return maxOrder;
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent) =>
        DoLayout(extents, order, parent, RenderChildDelegate, false);
}