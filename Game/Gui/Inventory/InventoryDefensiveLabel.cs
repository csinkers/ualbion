using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryDefensiveLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryDefensiveLabel, InventoryChangedEvent>((x,e) => x._version++),
            H<InventoryDefensiveLabel, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventoryDefensiveLabel, UiBlurEvent>((x, _) => x.Raise(new HoverTextEvent(""))));


        public InventoryDefensiveLabel(PartyCharacterId activeCharacter) : base(Handlers)
        {
            _activeCharacter = activeCharacter;
            var source = new DynamicText(() =>
            {
                var state = Resolve<IStateManager>();
                var player = state.State.GetPartyMember(_activeCharacter);
                var protection = player.BaseProtection; // TODO: Include items!
                return new[] { new TextBlock($": {protection}") };
            }, () => _version);

            Children.Add(
                new ButtonFrame(
                        new HorizontalStack(
                            new FixedSize(8, 8, new UiSprite<CoreSpriteId>(CoreSpriteId.UiDefensiveValue)),
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
            var protection = player.BaseProtection; // TODO: Include items!

            var template = assets.LoadString(SystemTextId.Inv_ProtectionN, settings.Language);
            var (text, _) = new TextFormatter(assets, settings.Language).Format(
                template, // Protection : %d
                protection);

            Raise(new HoverTextEvent(text.First().Text));
        }
    }
}