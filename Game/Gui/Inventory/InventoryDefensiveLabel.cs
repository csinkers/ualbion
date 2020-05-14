using System.Linq;
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
            On<BlurEvent>(e => Raise(new HoverTextEvent("")));
            On<HoverEvent>(e =>
            {
                Hover();
                e.Propagating = false;
            });

            _activeCharacter = activeCharacter;

            var source = new DynamicText(() =>
            {
                var player = Resolve<IParty>()[_activeCharacter];
                var protection = player?.Apparent.Combat.Protection ?? 0;
                return new[] { new TextBlock($": {protection}") };
            }, x => _version);

            AttachChild(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8,
                                new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiDefensiveValue) { Flags = SpriteFlags.Highlight }),
                            new TextElement(source)
                        )
                    )
                {
                    State = ButtonState.Pressed,
                    Padding = 0
                }
            );
        }

        void Hover()
        {
            var player = Resolve<IParty>()[_activeCharacter];
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            var protection = player?.Effective.Combat.Protection ?? 0;
            var template = assets.LoadString(SystemTextId.Inv_ProtectionN, settings.Gameplay.Language);
            var text = new TextFormatter(assets, settings.Gameplay.Language).Format(
                template, // Protection : %d
                protection).Blocks;

            Raise(new HoverTextEvent(text.First().Text));
        }
    }
}
