using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status
{
    public class StatusBar : Dialog
    {
        const int MaxPortraits = SavedGame.MaxPartySize;
        readonly UiSpriteElement<SlabId> _sprite;
        readonly StatusBarPortrait[] _portraits;
        readonly TextElement _hoverText;
        readonly TextElement _descriptionText;
        readonly FixedPosition _hoverTextContainer;
        readonly FixedPosition _descriptionTextContainer;

        public StatusBar() : base(DialogPositioning.StatusBar, int.MaxValue - 1) // MaxValue is reserved for context menu
        {
            On<HoverTextEvent>(e => _hoverText.LiteralString(e.Text));
            On<DescriptionTextEvent>(e => _descriptionText.LiteralString(e.Text));
            On<HoverTextExEvent>(e => _hoverText.Source(e.Source));
            On<DescriptionTextExEvent>(e => _descriptionText.Source(e.Source));

            _sprite = AttachChild(new UiSpriteElement<SlabId>(SlabId.SLAB));
            _sprite.SubId = 1;
            _portraits = new StatusBarPortrait[MaxPortraits];
            for (int i = 0; i < _portraits.Length; i++)
            {
                _portraits[i] = new StatusBarPortrait(i);
                Children.Add(_portraits[i]);
            }

            _hoverText = new TextElement("").Center().NoWrap();
            _descriptionText = new TextElement("").Center();
            _hoverTextContainer = AttachChild(new FixedPosition(new Rectangle(181, 196, 177, 10), _hoverText));
            _descriptionTextContainer = AttachChild(new FixedPosition(new Rectangle(181, 208, 177, 30), _descriptionText));
        }

        public override Vector2 GetSize() => new Vector2(UiConstants.StatusBarExtents.Width, UiConstants.StatusBarExtents.Height);

        int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func, bool trimOverlap)
        {
            int maxOrder = order;
            maxOrder = Math.Max(maxOrder, func(_sprite, extents, order + 1));
            maxOrder = Math.Max(maxOrder, func(_hoverTextContainer, extents, order + 2));
            maxOrder = Math.Max(maxOrder, func(_descriptionTextContainer, extents, order + 2));

            var party = Resolve<IParty>();
            if (party == null)
                return maxOrder;

            for (int i = 0; i < _portraits.Length; i++)
            {
                if (i >= party.StatusBarOrder.Count)
                    break;

                var portrait = _portraits[i];
                var portraitExtents = new Rectangle(
                    extents.X + 4 + 28 * i,
                    extents.Y + 3,
                    (int)portrait.GetSize().X - (trimOverlap ? 6 : 0),
                    (int)portrait.GetSize().Y);
                maxOrder = Math.Max(maxOrder, func(portrait, portraitExtents, order + 2));
            }
            return maxOrder;
        }

        public override int Select(
            Vector2 uiPosition,
            Rectangle extents,
            int order,
            Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            int maxOrder = DoLayout(extents,
                order,
                (x, y, z) =>  x.Select(uiPosition, y, z, registerHitFunc),
                true);
            registerHitFunc(order, this);
            return maxOrder;
        }

        public override int Render(Rectangle extents, int order) =>
            DoLayout(extents, order, (x, y, z) => x.Render(y, z), false);
    }
}
