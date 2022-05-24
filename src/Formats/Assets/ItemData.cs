using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public sealed class ItemData : IItem
{
    public const int SizeOnDisk = 0x28;
    public ItemData(ItemId id) => Id = id;
    [JsonIgnore] public StringId Name => Id.ToName();
    [JsonIgnore] public ItemId Id { get; }
    public byte Unknown { get; set; } //  0 Always 0
    public ItemType TypeId { get; set; } //  1 Item type
    public ItemSlotId SlotType { get; set; } //  2 Slot that can hold the item
    public byte BreakRate { get; set; } //  3 Chance to break the item
    [DefaultValue(Genders.Any)] public Genders AllowedGender { get; set; } //  4 Determines which gender can use this item. 2 = female, 3 = any
    [DefaultValue(1)] public byte Hands { get; set; } //  5 Determines how many free hands are required to equip the item.
    public byte LpMaxBonus { get; set; } //  6 Bonus value to life points.
    public byte SpMaxBonus { get; set; } //  7 Bonus value to spell points.
    public Attribute AttributeType { get; set; } //  8 Attribute bonus type
    public byte AttributeBonus { get; set; } //  9 Attribute bonus value.
    public Skill SkillType { get; set; } // 10 Skill bonus type
    public byte SkillBonus { get; set; } // 11 Skill bonus value.
    public byte Protection { get; set; } // 12 Protection from physical damage.
    public byte Damage { get; set; } // 13 Physical damage caused.
    public AmmunitionType AmmoType { get; set; } // 14 Ammunition type.
    public Skill SkillTax1Type { get; set; } // 15 Skill Tax 1 Type
    public Skill SkillTax2Type { get; set; } // 16 Skill Tax 2 Type
    public byte SkillTax1 { get; set; } // 17 Skill Tax 1 value. Ranged Values
    public byte SkillTax2 { get; set; } // 18 Skill Tax 2 value. Ranged Values
    public byte Activate { get; set; } // 19 Activate enables compass (0), monster eye (1) or clock (3) (if type=0×13) / Torch intensity (if type=0×16)
    public byte AmmoAnim { get; set; } // 20 Ammo combat animation (long-range weapons only)
    public SpellId Spell { get; set; } // 21 Spell (1 byte for class, 1 byte for number in class)
    public byte Charges { get; set; } // 23 Charges left in item / Torch lifetime (if type=0×16)
    public byte EnchantmentCount { get; set; } // 24 Number of times item was enchanted/recharged
    public byte MaxEnchantmentCount { get; set; } // 25 Maximum possible enchantments
    public byte MaxCharges { get; set; } // 26 Maximum number of charges
    public ItemFlags Flags { get; set; } // 27 Switch for vital, stackable and single-use items
    [DefaultValue(1)] public byte IconAnim { get; set; } // 29 Number of animated images
    public ushort Weight { get; set; } // 30 weight of the item in grams
    public ushort Value { get; set; } // 32 Base resell value * 10.
    public SpriteId Icon { get; set; }
    public int IconSubId { get; set; } // 34 Image for the item
    [DefaultValue(PlayerClasses.Anyone)] public PlayerClasses Class { get; set; } // 36 A bitfield that controls which classes can use the item.
    [DefaultValue(0xffff)] public ushort Race { get; set; } // 38 Likely meant to control which race can use the item – but does not seem to work ?
    [JsonIgnore] public bool IsStackable => (Flags & ItemFlags.Stackable) != 0;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Id,-19} {TypeId} {SlotType}"); 

        if (AttributeBonus != 0)
            sb.Append($"{AttributeType}+{AttributeBonus} ");

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

        if(AllowedGender != Genders.Any)
        {
            sb.Append(AllowedGender.ToString());
            sb.Append("Only ");
        }

        if (Damage != 0) sb.Append($"D:{Damage} ");
        if (Protection != 0) sb.Append($"P:{Protection} ");
        if (BreakRate != 0) sb.Append($"BR:{BreakRate} "); 
        if (Activate != 0) sb.Append($"A:{Activate} ");

        if (!Spell.IsNone)
            sb.Append($"S:{Spell} ");

        if (Charges != 0) sb.Append($"C:{Charges} ");
        if (MaxCharges != 0) sb.Append($"MaxC:{MaxCharges} ");

        if(EnchantmentCount != 0 || MaxEnchantmentCount != 0)
            sb.Append($"E:{EnchantmentCount} MaxE:{MaxEnchantmentCount} ");

        if(Flags != 0)
            sb.Append($"F:{Flags} ".Replace(", ", "|", StringComparison.InvariantCulture));

        if (Value != 0)
            sb.Append($"${(decimal)Value / 10:F}");

        return sb.ToString();
    }

    public static ItemData Serdes(AssetInfo info, ItemData item, ISerializer s, ISpellManager spellManager)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (spellManager == null) throw new ArgumentNullException(nameof(spellManager));

        item ??= new ItemData(info.AssetId);
        item.Unknown  = s.UInt8(nameof(item.Unknown), item.Unknown); // 0
        item.TypeId   = s.EnumU8(nameof(item.TypeId), item.TypeId);   // 1
        item.SlotType = ((PersistedItemSlotId)s.UInt8(nameof(item.SlotType), (byte)item.SlotType.ToPersisted())).ToMemory(); // 2
        item.BreakRate      = s.UInt8(nameof(item.BreakRate), item.BreakRate);           // 3
        item.AllowedGender  = s.EnumU8(nameof(item.AllowedGender), item.AllowedGender);  // 4
        item.Hands          = s.UInt8(nameof(item.Hands), item.Hands);                   // 5
        item.LpMaxBonus     = s.UInt8(nameof(item.LpMaxBonus), item.LpMaxBonus);         // 6
        item.SpMaxBonus     = s.UInt8(nameof(item.SpMaxBonus), item.SpMaxBonus);         // 7
        item.AttributeType  = s.EnumU8(nameof(item.AttributeType), item.AttributeType);  // 8
        item.AttributeBonus = s.UInt8(nameof(item.AttributeBonus), item.AttributeBonus); // 9
        item.SkillType      = s.EnumU8(nameof(item.SkillType), item.SkillType);          // A
        item.SkillBonus     = s.UInt8(nameof(item.SkillBonus), item.SkillBonus);         // B
        item.Protection     = s.UInt8(nameof(item.Protection), item.Protection);         // C
        item.Damage         = s.UInt8(nameof(item.Damage), item.Damage);                 // D
        item.AmmoType       = s.EnumU8(nameof(item.AmmoType), item.AmmoType);            // E
        item.SkillTax1Type  = s.EnumU8(nameof(item.SkillTax1Type), item.SkillTax1Type);  // F
        item.SkillTax2Type  = s.EnumU8(nameof(item.SkillTax2Type), item.SkillTax2Type);  // 10
        item.SkillTax1      = s.UInt8(nameof(item.SkillTax1), item.SkillTax1);           // 11
        item.SkillTax2      = s.UInt8(nameof(item.SkillTax2), item.SkillTax2);           // 12
        item.Activate       = s.UInt8(nameof(item.Activate), item.Activate);             // 13
        item.AmmoAnim       = s.UInt8(nameof(item.AmmoAnim), item.AmmoAnim);             // 14

        var spell = item.Spell.IsNone ? null : spellManager.GetSpellOrDefault(item.Spell);
        SpellClass spellClass = s.EnumU8("SpellClass", spell?.Class ?? 0);                  // 15
        byte spellNumber = s.UInt8("SpellNumber", (byte)((spell?.OffsetInClass + 1) ?? 0)); // 16
        item.Spell = spellNumber == 0 
            ? SpellId.None 
            : spellManager.GetSpellId(spellClass, (byte)(spellNumber - 1));

        item.Charges             = s.UInt8(nameof(item.Charges), item.Charges);                         // 17
        item.EnchantmentCount    = s.UInt8(nameof(item.EnchantmentCount), item.EnchantmentCount);       // 18
        item.MaxEnchantmentCount = s.UInt8(nameof(item.MaxEnchantmentCount), item.MaxEnchantmentCount); // 19
        item.MaxCharges          = s.UInt8(nameof(item.MaxCharges), item.MaxCharges);                   // 1A
        item.Flags               = s.EnumU16(nameof(item.Flags), item.Flags);                           // 1B
        item.IconAnim            = s.UInt8(nameof(item.IconAnim), item.IconAnim);                       // 1D
        item.Weight              = s.UInt16(nameof(item.Weight), item.Weight);                          // 1E
        item.Value               = s.UInt16(nameof(item.Value), item.Value);                            // 20
        item.Icon                = SpriteId.From(Base.ItemGfx.ItemSprites); // TODO: Allow mods to add extra sprite sheets via specifying their ID in the AssetInfo.
        item.IconSubId           = s.UInt16(nameof(item.IconSubId), (ushort)item.IconSubId);            // 22
        item.Class               = s.EnumU16(nameof(item.Class), item.Class);                           // 24
        item.Race                = s.UInt16(nameof(item.Race), item.Race);                              // 26
        return item; // Total size 0x28
    }

    bool Equals(ItemData other) => Id == other.Id;
    public bool Equals(IContents obj) => Equals((object) obj);
    public override bool Equals(object obj) => obj is ItemData other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
}
