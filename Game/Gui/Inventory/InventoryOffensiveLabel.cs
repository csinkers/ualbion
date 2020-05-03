using System.Linq;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryOffensiveLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        int _version;

        public InventoryOffensiveLabel(PartyCharacterId activeCharacter)
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
                var damage = player?.Apparent.Combat.Damage ?? 0;
                return new[] { new TextBlock($": {damage}") };
            }, x => _version);

            AttachChild(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8,
                                new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiOffensiveValue) { Highlighted = true }),
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

            var damage = player?.Effective.Combat.Damage ?? 0;
            var template = assets.LoadString(SystemTextId.Inv_DamageN, settings.Gameplay.Language);
            var text = new TextFormatter(assets, settings.Gameplay.Language).Format(
                template, // Damage : %d
                damage).Blocks;

            Raise(new HoverTextEvent(text.First().Text));
        }
    }
}
