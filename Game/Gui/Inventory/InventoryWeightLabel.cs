using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryWeightLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly DynamicText _hoverSource;
        int _version = 0;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryWeightLabel, InventoryChangedEvent>((x,e) => x._version++),
            H<InventoryWeightLabel, SetLanguageEvent>((x, e) => x._version++),
            H<InventoryWeightLabel, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventoryWeightLabel, UiBlurEvent>((x, _) => x.Raise(new HoverTextEvent(""))));

        public InventoryWeightLabel(PartyCharacterId activeCharacter) : base(Handlers)
        {
            _activeCharacter = activeCharacter;

            _hoverSource = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var characterManager = Resolve<ICharacterManager>();

                int weight = characterManager.GetTotalWeight(_activeCharacter);
                var maxWeight = characterManager.GetMaxWeight(_activeCharacter);

                // Carried Weight : %ld of %ld g
                var template = assets.LoadString(SystemTextId.Inv_CarriedWeightNdOfNdG, settings.Language);
                var (text, _) = new TextFormatter(assets, settings.Language).Format(template, weight, maxWeight);
                return text;
            }, x => _version);

            var source = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var characterManager = Resolve<ICharacterManager>();

                int weight = characterManager.GetTotalWeight(_activeCharacter) / 1000;
                var template = assets.LoadString(SystemTextId.Inv_WeightNKg, settings.Language); // Weight : %d Kg
                return new 
                    TextFormatter(assets, settings.Language)
                    .NoWrap()
                    .Centre()
                    .Format(template, weight)
                    .Item1;
            }, x => _version);

            Children.Add(new ButtonFrame(new Text(source))
            {
                State = ButtonState.Pressed,
                Padding = 0
            });

        }

        void Hover() => Raise(new HoverTextEvent(_hoverSource.Get().First().Text));
    }
}