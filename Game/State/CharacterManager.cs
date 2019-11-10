using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.State
{
    public class CharacterManager : Component, ICharacterManager
    {
        public int GetTotalWeight(PartyCharacterId id)
        {
            var assets = Resolve<IAssetManager>();
            var state = Resolve<IStateManager>().State;
            var member = state.GetPartyMember(id);

            int weight = 0;
            foreach (var itemSlot in member.Inventory.EnumerateAll())
            {
                var item = assets.LoadItem(itemSlot.Id);
                weight += item.Weight * itemSlot.Amount;
            }

            weight += member.Inventory.Gold * 50; // TODO: Check actual values
            weight += member.Inventory.Rations * 100;

            return weight;
        }

        public int GetMaxWeight(PartyCharacterId id)
        {
            var state = Resolve<IStateManager>().State;
            var member = state.GetPartyMember(id);
            return 5000 + 3000 * member.Attributes.Strength; // TODO: Use actual values
        }

        public int GetTotalDamage(PartyCharacterId id)
        {
            var assets = Resolve<IAssetManager>();
            var state = Resolve<IStateManager>().State;
            var member = state.GetPartyMember(id);

            int damage = 0;
            foreach (var itemSlot in member.Inventory.EnumerateBodyParts())
            {
                var item = assets.LoadItem(itemSlot.Id);
                damage += item.Damage;
            }

            return damage;
        }

        public int GetTotalProtection(PartyCharacterId id)
        {
            var assets = Resolve<IAssetManager>();
            var state = Resolve<IStateManager>().State;
            var member = state.GetPartyMember(id);

            int protection = 0;
            foreach (var itemSlot in member.Inventory.EnumerateBodyParts())
            {
                var item = assets.LoadItem(itemSlot.Id);
                protection += item.Protection;
            }

            return protection;
        }
    }
}