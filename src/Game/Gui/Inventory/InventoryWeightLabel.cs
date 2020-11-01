using System;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryWeightLabel : UiElement
    {
        readonly PartyMemberId _activeCharacter;
        readonly IText _hoverSource;
        int _version;

        public InventoryWeightLabel(PartyMemberId activeCharacter)
        {
            On<LanguageChangedEvent>(e => _version++);
            On<BlurEvent>(e => Raise(new HoverTextEvent(null)));
            On<InventoryChangedEvent>(e =>
            {
                if (_activeCharacter == e.Id.ToAssetId())
                    _version++;
            });
            On<HoverEvent>(e =>
            {
                Hover();
                e.Propagating = false;
            });

            _activeCharacter = activeCharacter;

            _hoverSource = new DynamicText(() =>
            {
                var player = Resolve<IParty>()[_activeCharacter];
                if (player == null)
                    return Array.Empty<TextBlock>();

                // Carried Weight : %ld of %ld g
                return Resolve<ITextFormatter>().Format(
                    Base.SystemText.Inv_CarriedWeightNdOfNdG,
                    player.Apparent.TotalWeight,
                    player.Apparent.MaxWeight).GetBlocks();
            }, x => _version);

            var source = new DynamicText(() =>
            {
                var player = Resolve<IParty>()[_activeCharacter];
                if (player == null)
                    return Array.Empty<TextBlock>();

                // Weight : %d Kg
                int weight = player.Apparent.TotalWeight / 1000;
                return Resolve<ITextFormatter>()
                    .NoWrap()
                    .Center()
                    .Format(Base.SystemText.Inv_WeightNKg, weight)
                    .GetBlocks();
            }, x => _version);

            AttachChild(
                new ButtonFrame(
                    new FixedSize(66, 8,
                        new HorizontalStack(
                            new Spacing(1, 0),
                            new UiText(source)
                        )
                    )
                )
                {
                    State = ButtonState.Pressed,
                    Padding = 0
                });
        }

        void Hover() => Raise(new HoverTextEvent(_hoverSource));
    }
}
