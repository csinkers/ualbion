using System.Linq;
using UAlbion.Core.Events;
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
            H<InventoryDefensiveLabel, HoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventoryDefensiveLabel, BlurEvent>((x, _) => x.Raise(new HoverTextEvent(""))));

        public InventoryDefensiveLabel(PartyCharacterId activeCharacter) : base(Handlers)
        {
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
                                new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiDefensiveValue) { Highlighted = true }),
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
