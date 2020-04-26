using System.Linq;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryWeightLabel : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly DynamicText _hoverSource;
        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryWeightLabel, InventoryChangedEvent>((x, e) =>
            {
                if (e.InventoryType == InventoryType.Player && x._activeCharacter == (PartyCharacterId)e.InventoryId)
                    x._version++;
            }),
            H<InventoryWeightLabel, SetLanguageEvent>((x, e) => x._version++),
            H<InventoryWeightLabel, HoverEvent>((x, e) =>
            {
                x.Hover();
                e.Propagating = false;
            }),
            H<InventoryWeightLabel, BlurEvent>((x, _) => x.Raise(new HoverTextEvent(""))));

        public InventoryWeightLabel(PartyCharacterId activeCharacter) : base(Handlers)
        {
            _activeCharacter = activeCharacter;

            _hoverSource = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var player = Resolve<IParty>()[_activeCharacter];
                if(player == null)
                    return new TextBlock[0];

                // Carried Weight : %ld of %ld g
                var template = assets.LoadString(SystemTextId.Inv_CarriedWeightNdOfNdG, settings.Gameplay.Language);
                return new TextFormatter(assets, settings.Gameplay.Language).Format(template, player.Apparent.TotalWeight, player.Apparent.MaxWeight).Blocks;
            }, x => _version);

            var source = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var player = Resolve<IParty>()[_activeCharacter];
                if(player == null)
                    return new TextBlock[0];

                int weight = player.Apparent.TotalWeight / 1000;
                var template = assets.LoadString(SystemTextId.Inv_WeightNKg, settings.Gameplay.Language); // Weight : %d Kg
                return new
                    TextFormatter(assets, settings.Gameplay.Language)
                    .NoWrap()
                    .Centre()
                    .Format(template, weight)
                    .Blocks;
            }, x => _version);

            AttachChild(new ButtonFrame(new TextElement(source))
            {
                State = ButtonState.Pressed,
                Padding = 0
            });
        }

        void Hover() => Raise(new HoverTextEvent(_hoverSource.Get().FirstOrDefault()?.Text));
    }
}
