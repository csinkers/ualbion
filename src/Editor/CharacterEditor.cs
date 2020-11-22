using System;
using ImGuiNET;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
{
    public class CharacterEditor : AssetEditor
    {
        readonly string _name;
        readonly CharacterSheet _sheet;
        readonly CharacterMagicEditor _magic;
        readonly InventoryEditor _inventory;
        readonly CharacterAttributesEditor _attributes;
        readonly CharacterSkillsEditor _skills;
        readonly CombatAttributeEditor _combat;

        public CharacterEditor(string name, CharacterSheet sheet) : base(sheet)
        {
            _name = name;
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _magic = AttachChild(new CharacterMagicEditor(_sheet.Magic));
            _inventory = AttachChild(new InventoryEditor(_sheet.Inventory));
            _attributes = AttachChild(new CharacterAttributesEditor(_sheet.Attributes));
            _skills = AttachChild(new CharacterSkillsEditor(_sheet.Skills));
            _combat = AttachChild(new CombatAttributeEditor(_sheet.Combat));
        }

        public override void Render()
        {
            if (!ImGui.Begin(_name)) return;

            ImGui.Text(_sheet.Id.ToString());
            if (ImGui.TreeNode("Magic")) { _magic.Render(); ImGui.TreePop(); }
            if (ImGui.TreeNode("Inventory")) { _inventory.Render(); ImGui.TreePop(); } 
            if (ImGui.TreeNode("Attributes")) { _attributes.Render(); ImGui.TreePop(); } 
            if (ImGui.TreeNode("Skills")) { _skills.Render(); ImGui.TreePop(); } 
            if (ImGui.TreeNode("Combat")) { _combat.Render(); ImGui.TreePop(); }

            if (ImGui.TreeNode("Misc"))
            {
                ImGui.Text("Note: If the English / French names are left blank, they will default to the German one.");
                Text(nameof(_sheet.EnglishName), _sheet.EnglishName, CharacterSheet.MaxNameLength);
                Text(nameof(_sheet.GermanName), _sheet.GermanName, CharacterSheet.MaxNameLength);
                Text(nameof(_sheet.FrenchName), _sheet.FrenchName, CharacterSheet.MaxNameLength);

                // CharacterType Type // Consequence of the AssetType so not configurable. Worth displaying, or will it be obvious?
                EnumRadioButtons(nameof(_sheet.Gender), _sheet.Gender);
                EnumRadioButtons(nameof(_sheet.Race), _sheet.Race); // TODO: Dropdown
                EnumRadioButtons(nameof(_sheet.PlayerClass), _sheet.PlayerClass); // TODO: Dropdown
                UInt16Slider(nameof(_sheet.Age), _sheet.Age, 0, ushort.MaxValue);
                UInt8Slider(nameof(_sheet.Level), _sheet.Level, 0, byte.MaxValue);

                EnumCheckboxes(nameof(_sheet.Languages), _sheet.Languages);
                ImGui.Text($"SpriteId: {_sheet.SpriteId}"); // TODO: Combo-box? Resource picker?

                ImGui.Text($"PortraitId: {_sheet.PortraitId} ({_sheet.PortraitId.Id})"); // TODO: Combo-box? Resource picker?
                ImGui.Text($"EventSet: {_sheet.EventSetId} ({_sheet.EventSetId.Id})"); // TODO: Combo-box? Resource picker?
                ImGui.Text($"WordSet: {_sheet.WordSetId} ({_sheet.WordSetId.Id})"); // TODO: Combo-box? Resource picker?
                ImGui.TreePop();
            }

            ImGui.End();
        }
    }
}
