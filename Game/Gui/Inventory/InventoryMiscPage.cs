using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryMiscPage : UiElement
    {
        const string CombatPositionButtonId = "Inventory.CombatPositions";

        public InventoryMiscPage(PartyCharacterId activeCharacter)
        {
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);
            var stack = new VerticalStack(
                new Header(S(SystemTextId.Inv3_Conditions)),
                new Spacing(0, 64),
                new Header(S(SystemTextId.Inv3_Languages)),
                new Spacing(0, 23),
                new Header(S(SystemTextId.Inv3_TemporarySpells)),
                new Spacing(0, 45),
                new Button(CombatPositionButtonId, S(SystemTextId.Inv3_CombatPositions))
            );
            AttachChild(stack);
        }
    }
}
