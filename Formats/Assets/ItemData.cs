using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public sealed class ItemData : IItem
    {
        public ItemData(ItemId id) => Id = id;
        public ItemId Id { get; }
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
        public SpellClass SpellClass { get; set; }   // 21 Spell Class
        public byte? SpellEffect { get; set; }   // 22 Spell Id
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

        public bool IsStackable => (Flags & ItemFlags.Stackable) != 0;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(((int)Id).ToString().PadLeft(3)); sb.Append(' ');
            sb.Append(Id.ToString().PadRight(19)); sb.Append(' ');
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

            if (Damage != 0) sb.AppendFormat("D:{0} ", Damage);
            if (Protection != 0) sb.AppendFormat("P:{0} ", Protection);
            if (BreakRate != 0) sb.AppendFormat("BR:{0} ", BreakRate); 
            if (Activate != 0) sb.AppendFormat("A:{0} ", Activate);

            if (SpellEffect.HasValue)
            {
                var className = SpellClass.ToString().Replace(", ", "|");
                SpellId spellId = SpellClass.ToSpellId(SpellEffect.Value);
                sb.AppendFormat($"SC:{className} SE:{SpellEffect}={spellId} ");
            }

            if (Charges != 0) sb.AppendFormat("C:{0} ", Charges);
            if (MaxCharges != 0) sb.AppendFormat("MaxC:{0} ", MaxCharges);

            if(EnchantmentCount != 0 || MaxEnchantmentCount != 0)
                sb.AppendFormat("E:{0} MaxE:{1} ", EnchantmentCount, MaxEnchantmentCount);

            if(Flags != 0)
                sb.AppendFormat("F:{0} ", Flags.ToString().Replace(", ", "|"));

            if (Value != 0)
                sb.Append($"${(decimal)Value / 10:F}");

            return sb.ToString();
        }

        public static ItemData Serdes(int i, ItemData item, ISerializer s)
        {
            item ??= new ItemData((ItemId)i);
            ApiUtil.Assert(i == (int) item.Id);
            item.Unknown = s.UInt8(nameof(item.Unknown), item.Unknown);
            item.TypeId = s.EnumU8(nameof(item.TypeId), item.TypeId);
            item.SlotType = ((PersistedItemSlotId)s.UInt8(nameof(item.SlotType), (byte)item.SlotType.ToPersisted())).ToMemory();
            item.BreakRate = s.UInt8(nameof(item.BreakRate), item.BreakRate);
            item.AllowedGender = s.EnumU8(nameof(item.AllowedGender), item.AllowedGender);
            item.Hands = s.UInt8(nameof(item.Hands), item.Hands);
            item.LpMaxBonus = s.UInt8(nameof(item.LpMaxBonus), item.LpMaxBonus);
            item.SpMaxBonus = s.UInt8(nameof(item.SpMaxBonus), item.SpMaxBonus);
            item.AttributeType = s.EnumU8(nameof(item.AttributeType), item.AttributeType);
            item.AttributeBonus = s.UInt8(nameof(item.AttributeBonus), item.AttributeBonus);
            item.SkillType = s.EnumU8(nameof(item.SkillType), item.SkillType);
            item.SkillBonus = s.UInt8(nameof(item.SkillBonus), item.SkillBonus);
            item.Protection = s.UInt8(nameof(item.Protection), item.Protection);
            item.Damage = s.UInt8(nameof(item.Damage), item.Damage);
            item.AmmoType = s.EnumU8(nameof(item.AmmoType), item.AmmoType);
            item.SkillTax1Type = s.EnumU8(nameof(item.SkillTax1Type), item.SkillTax1Type);
            item.SkillTax2Type = s.EnumU8(nameof(item.SkillTax2Type), item.SkillTax2Type);
            item.SkillTax1 = s.UInt8(nameof(item.SkillTax1), item.SkillTax1);
            item.SkillTax2 = s.UInt8(nameof(item.SkillTax2), item.SkillTax2);
            item.Activate = s.UInt8(nameof(item.Activate), item.Activate);
            item.AmmoAnim = s.UInt8(nameof(item.AmmoAnim), item.AmmoAnim);
            item.SpellClass = s.EnumU8(nameof(item.SpellClass), item.SpellClass);
            item.SpellEffect = StoreIncrementedNullZero.Serdes(nameof(item.SpellEffect), item.SpellEffect, s.UInt8);
            item.Charges = s.UInt8(nameof(item.Charges), item.Charges);
            item.EnchantmentCount = s.UInt8(nameof(item.EnchantmentCount), item.EnchantmentCount);
            item.MaxEnchantmentCount = s.UInt8(nameof(item.MaxEnchantmentCount), item.MaxEnchantmentCount);
            item.MaxCharges = s.UInt8(nameof(item.MaxCharges), item.MaxCharges);
            item.Flags = s.EnumU16(nameof(item.Flags), item.Flags);
            item.IconAnim = s.UInt8(nameof(item.IconAnim), item.IconAnim);
            item.Weight = s.UInt16(nameof(item.Weight), item.Weight);
            item.Value = s.UInt16(nameof(item.Value), item.Value);
            item.Icon = s.EnumU16(nameof(item.Icon), item.Icon);
            item.Class = s.EnumU16(nameof(item.Class), item.Class);
            item.Race = s.UInt16(nameof(item.Race), item.Race);
            return item;
        }

        bool Equals(ItemData other) => Id == other.Id;
        public bool Equals(IContents obj) => Equals((object) obj);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ItemData) obj);
        }

        public override int GetHashCode() => (int) Id;
    }
}
