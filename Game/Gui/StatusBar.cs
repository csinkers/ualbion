using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class StatusBar : Dialog
    {
        const int MaxPortraits = 6;
        readonly UiSprite<PictureId> _sprite;
        readonly StatusBarPortrait[] _portraits;
        readonly Text _hoverText;
        readonly Text _descriptionText;
        readonly FixedPosition _hoverTextContainer;
        readonly FixedPosition _descriptionTextContainer;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<StatusBar, HoverTextEvent>((x,e) => x._hoverText.LiteralString(e.Text)),
            H<StatusBar, DescriptionTextEvent>((x,e) => x._descriptionText.LiteralString(e.Text))
        );

        public StatusBar() : base(Handlers, DialogPositioning.StatusBar, int.MaxValue)
        {
            _sprite = new UiSprite<PictureId>(PictureId.StatusBar);
            Children.Add(_sprite);
            _portraits = new StatusBarPortrait[MaxPortraits];
            for (int i = 0; i < _portraits.Length; i++)
            {
                _portraits[i] = new StatusBarPortrait(i);
                Children.Add(_portraits[i]);
            }

            _hoverText = new Text("").Center().NoWrap();
            _descriptionText = new Text("").Center();
            _hoverTextContainer = new FixedPosition(new Rectangle(181, 196, 177, 10), _hoverText);
            _descriptionTextContainer = new FixedPosition(new Rectangle(181, 208, 177, 30), _descriptionText);
            Children.Add(_hoverTextContainer);
            Children.Add(_descriptionTextContainer);
        }

        public override Vector2 GetSize() => new Vector2(UiConstants.StatusBarExtents.Width, UiConstants.StatusBarExtents.Height);

        int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func, bool trimOverlap)
        {
            int maxOrder = order;
            maxOrder = Math.Max(maxOrder, func(_sprite, extents, order + 1));
            maxOrder = Math.Max(maxOrder, func(_hoverTextContainer, extents, order + 1));
            maxOrder = Math.Max(maxOrder, func(_descriptionTextContainer, extents, order + 1));

            var stateManager = Resolve<IStateManager>();
            if (stateManager.State == null)
                return maxOrder;

            for (int i = 0; i < _portraits.Length; i++)
            {
                if (i >= stateManager.State.Party.Players.Count)
                    break;

                var portrait = _portraits[i];
                var portraitExtents = new Rectangle(
                    extents.X + 4 + 28 * i,
                    extents.Y + 3,
                    (int)portrait.GetSize().X - (trimOverlap ? 6 : 0),
                    (int)portrait.GetSize().Y);
                maxOrder = Math.Max(maxOrder, func(portrait, portraitExtents, order + 1));
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

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) =>
            DoLayout(extents, order, (x, y, z) => x.Render(y, z, addFunc), false);
    }
}
