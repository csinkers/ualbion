using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryDefensiveLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        int _version;

        public InventoryDefensiveLabel(PartyCharacterId activeCharacter)
        {
            On<InventoryChangedEvent>(e => _version++);
            On<BlurEvent>(e => Raise(new HoverTextEvent(null)));
            On<HoverEvent>(Hover);

            _activeCharacter = activeCharacter;

            var source = new DynamicText(() =>
            {
                var player = Resolve<IParty>()[_activeCharacter];
                var protection = player?.Apparent.DisplayProtection ?? 0;
                return new[] { new TextBlock($": {protection}") };
            }, x => _version);

            AttachChild(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8,
                                new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiDefensiveValue) { Flags = SpriteFlags.Highlight }),
                            new UiText(source)
                        )
                    )
                {
                    State = ButtonState.Pressed,
                    Padding = 0
                }
            );
        }

        void Hover(HoverEvent e)
        {
            var player = Resolve<IParty>()[_activeCharacter];
            var tf = Resolve<ITextFormatter>();

            // Protection : %d
            var protection = player?.Apparent.DisplayProtection ?? 0;
            var text = tf.Format(SystemTextId.Inv_ProtectionN.ToId(), protection);
            Raise(new HoverTextEvent(text));
            e.Propagating = false;
        }
    }
}
