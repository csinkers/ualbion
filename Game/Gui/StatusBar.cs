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

        public StatusBar() : base(Handlers, DialogPositioning.StatusBar, Int32.MaxValue)
        {
            _sprite = new UiSprite<PictureId>(PictureId.StatusBar);
            Children.Add(_sprite);
            _portraits = new StatusBarPortrait[MaxPortraits];
            for (int i = 0; i < _portraits.Length; i++)
            {
                _portraits[i] = new StatusBarPortrait(i);
                Children.Add(_portraits[i]);
            }

            _hoverText = new Text("").Center();
            _descriptionText = new Text("").Center();
            _hoverTextContainer = new FixedPosition(new Rectangle(181, 196, 177, 10), _hoverText);
            _descriptionTextContainer = new FixedPosition(new Rectangle(181, 208, 177, 30), _descriptionText);
            Children.Add(_hoverTextContainer);
            Children.Add(_descriptionTextContainer);
        }

        public override Vector2 GetSize() => new Vector2(UiConstants.StatusBarExtents.Width, UiConstants.StatusBarExtents.Height);

        void DoLayout(Rectangle extents, Action<Rectangle, IUiElement> action, bool trimOverlap)
        {
            action(extents, _sprite);
            action(extents, _hoverTextContainer);
            action(extents, _descriptionTextContainer);

            var stateManager = Resolve<IStateManager>();
            if (stateManager.State == null)
                return;

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
                action(portraitExtents, portrait);
            }
        }

        public override int Select(
            Vector2 uiPosition,
            Rectangle extents,
            int order,
            Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;

            int maxOrder = order;
            DoLayout(extents,
                (rect, x) => { maxOrder = Math.Max(maxOrder, x.Select(uiPosition, rect, order, registerHitFunc)); },
                true);
            registerHitFunc(order, this);
            return maxOrder;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            int maxOrder = order;
            DoLayout(extents,
                (rect, x) => { maxOrder = Math.Max(maxOrder, x.Render(rect, order, addFunc)); },
                false);
            return maxOrder;
        }
    }
}
