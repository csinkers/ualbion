using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.ItemData)]
    public class ItemDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            long end = br.BaseStream.Position + streamLength;
            var items = new List<ItemData>();
            int i = 0;
            while (br.BaseStream.Position < end)
            {
                var item = new ItemData();
                item.Id = (ItemId)i;
                item.Unknown = br.ReadByte();
                item.TypeId = (ItemType)br.ReadByte();
                item.SlotType = (ItemSlotId)br.ReadByte();
                item.BreakRate = br.ReadByte();
                item.AllowedGender = (GenderMask)br.ReadByte();
                item.Hands = br.ReadByte();
                item.LpMaxBonus = br.ReadByte();
                item.SpMaxBonus = br.ReadByte();
                item.AttributeType = (Attribute)br.ReadByte();
                item.AttributeBonus = br.ReadByte();
                item.SkillType = (Skill)br.ReadByte();
                item.SkillBonus = br.ReadByte();
                item.Protection = br.ReadByte();
                item.Damage = br.ReadByte();
                item.AmmoType = (AmmunitionType)br.ReadByte();
                item.SkillTax1Type = (Skill)br.ReadByte();
                item.SkillTax2Type = (Skill)br.ReadByte();
                item.SkillTax1 = br.ReadByte();
                item.SkillTax2 = br.ReadByte();
                item.Activate = br.ReadByte();
                item.AmmoAnim = br.ReadByte();
                item.SpellClass = (SpellClassMask)br.ReadByte();
                item.SpellEffect = br.ReadByte();
                item.Charges = br.ReadByte();
                item.EnchantmentCount = br.ReadByte();
                item.MaxEnchantmentCount = br.ReadByte();
                item.MaxCharges = br.ReadByte();
                item.Flags = (ItemFlags)br.ReadUInt16();
                item.IconAnim = br.ReadByte();
                item.Weight = br.ReadUInt16();
                item.Value = br.ReadUInt16();
                item.Icon = br.ReadUInt16();
                item.Class = (PlayerClassMask)br.ReadUInt16();
                item.Race = br.ReadUInt16();
                items.Add(item);
                i++;
            }

            return items;
        }
    }
}