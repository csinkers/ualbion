using System.Text;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class ItemData
    {
        public ItemId Id { get; set; }
        public string[] Names { get; set; }

        public string GetName(GameLanguage language) =>
            language switch
            {
                GameLanguage.German => Names[0],
                GameLanguage.English => Names[1],
                GameLanguage.French => Names[2],
                _ => Names[1]
            };

        public byte Unknown { get; set; }   //  0 Always 0
        public ItemType TypeId { get; set; }   //  1 Item type
        public ItemSlotId SlotType { get; set; }   //  2 Slot that can hold the item
        public byte BreakRate { get; set; }   //  3 Chance to break the item
        public GenderMask AllowedGender { get; set; }   //  4 Determines which gender can use this item. 2 = female, 3 = any
        public byte Hands { get; set; }   //  5 Determines how many free hands are required to equip the item.
        public byte LpMaxBonus { get; set; }   //  6 Bonus value to life points.
        public byte SpMaxBonus { get; set; }   //  7 Bonus value to spell points.
        public Attribute AttributeType { get; set; }   //  8 Attribute bonus type
        public byte AttributeBonus { get; set; }   //  9 Attribute bonus value.
        public Skill SkillType { get; set; }   // 10 Skill bonus type
        public byte SkillBonus { get; set; }   // 11 Skill bonus value.
        public byte Protection { get; set; }   // 12 Protection from physical damage.
        public byte Damage { get; set; }   // 13 Physical damage caused.
        public AmmunitionType AmmoType { get; set; }   // 14 Ammunition type.
        public Skill SkillTax1Type { get; set; }   // 15 Skill Tax 1 Type
        public Skill SkillTax2Type { get; set; }   // 16 Skill Tax 2 Type
        public byte SkillTax1 { get; set; }   // 17 Skill Tax 1 value. Ranged Values
        public byte SkillTax2 { get; set; }   // 18 Skill Tax 2 value. Ranged Values
        public byte Activate { get; set; }   // 19 Activate enables compass (0), monster eye (1) or clock (3) (if type=0×13) / Torch intensity (if type=0×16)
        public byte AmmoAnim { get; set; }   // 20 Ammo combat animation (long-range weapons only)
        public SpellClassMask SpellClass { get; set; }   // 21 Spell Class (TODO: Is this SpellClass or SpellClassMask?)
        public byte SpellEffect { get; set; }   // 22 Spell Id
        public byte Charges { get; set; }   // 23 Charges left in item / Torch lifetime (if type=0×16)
        public byte EnchantmentCount { get; set; }   // 24 Number of times item was enchanted/recharged
        public byte MaxEnchantmentCount { get; set; }   // 25 Maximum possible enchantments
        public byte MaxCharges { get; set; }   // 26 Maximum number of charges
        public ItemFlags Flags { get; set; }   // 27 Switch for vital, stackable and single-use items
        public byte IconAnim { get; set; }   // 29 Number of animated images
        public ushort Weight { get; set; }   // 30 weight of the item in grams
        public ushort Value { get; set; }   // 32 Base resell value * 10.
        public ItemSpriteId Icon { get; set; }   // 34 Image for the item
        public PlayerClassMask Class { get; set; }   // 36 A bitfield that controls which classes can use the item.
        public ushort Race { get; set; }   // 38 Likely meant to control which race can use the item – but does not seem to work ?

        public bool IsStackable =>
            TypeId == ItemType.Ammo
            //|| TypeId == ItemType.Document // ?
            //|| TypeId == ItemType.SpellScroll // ?
            || TypeId == ItemType.Drink
            //|| TypeId == ItemType.Amulet
            //|| TypeId == ItemType.MagicRing
            || TypeId == ItemType.Valuable // ?
            || TypeId == ItemType.Tool // ?
            || TypeId == ItemType.Key // ?
            || TypeId == ItemType.Misc
            //|| TypeId == ItemType.MagicItem
            //|| TypeId == ItemType.HeadsUpDisplayItem
            || TypeId == ItemType.Lockpick
            || TypeId == ItemType.LightSource;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Id.ToString()); sb.Append(' ');
            sb.Append(TypeId.ToString()); sb.Append(' ');
            sb.Append(SlotType.ToString()); sb.Append(' ');

            if (AttributeBonus != 0)
            {
                sb.Append(AttributeType.ToString());
                sb.Append('+');
                sb.Append(AttributeBonus);
                sb.Append(' ');
            }

            if (SkillBonus != 0)
            {
                sb.Append(SkillType.ToString());
                sb.Append('+');
                sb.Append(SkillBonus);
                sb.Append(' ');
            }

            if (SkillTax1 != 0)
            {
                sb.Append(SkillTax1Type.ToString());
                sb.Append('-');
                sb.Append(SkillTax1);
                sb.Append(' ');
            }

            if (SkillTax2 != 0)
            {
                sb.Append(SkillTax2Type.ToString());
                sb.Append('-');
                sb.Append(SkillTax2);
                sb.Append(' ');
            }

            if (AmmoType != AmmunitionType.Intrinsic)
            {
                sb.Append("Ammo:");
                sb.Append(AmmoType.ToString());
                sb.Append(' ');
            }

            if(LpMaxBonus != 0)
            {
                sb.Append("LP+");
                sb.Append(LpMaxBonus);
                sb.Append(' ');
            }
            if(SpMaxBonus != 0)
            {
                sb.Append("SP+");
                sb.Append(SpMaxBonus);
                sb.Append(' ');
            }

            if(AllowedGender != GenderMask.Any)
            {
                sb.Append(AllowedGender.ToString());
                sb.Append("Only ");
            }

            sb.AppendFormat("{0}D {1}P ", Damage, Protection);

            return sb.ToString();
        }
    }
}
