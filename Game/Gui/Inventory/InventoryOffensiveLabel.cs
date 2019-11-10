using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryOffensiveLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryOffensiveLabel, InventoryChangedEvent>((x, e) => x._version++),
            H<InventoryOffensiveLabel, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventoryOffensiveLabel, UiBlurEvent>((x, _) => x.Raise(new HoverTextEvent(""))));


        public InventoryOffensiveLabel(PartyCharacterId activeCharacter) : base(Handlers)
        {
            _activeCharacter = activeCharacter;
            var source = new DynamicText(() =>
            {
                var state = Resolve<IStateManager>();
                var player = state.State.GetPartyMember(_activeCharacter);
                var damage = player.BaseDamage; // TODO: Include items!
                return new[] { new TextBlock($": {damage}") };
            }, () => _version);

            Children.Add(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8, new UiSprite<CoreSpriteId>(CoreSpriteId.UiOffensiveValue)),
                            new Text(source)
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
            var state = Resolve<IStateManager>();
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            var player = state.State.GetPartyMember(_activeCharacter);
            var damage = player.BaseDamage; // TODO: Include items!

            var template = assets.LoadString(SystemTextId.Inv_DamageN, settings.Language);
            var (text, _) = new TextFormatter(assets, settings.Language).Format(
                template, // Damage : %d
                damage);

            Raise(new HoverTextEvent(text.First().Text));
        }
    }
}