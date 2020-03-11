using System.Collections.Generic;
using System.Text;

namespace UAlbion.Formats.Assets
{
    public class Chest : IChest
    {
        public const int SlotCount = 24;
        public ushort Gold { get; set; } // Not used for merchants
        public ushort Rations { get; set; } // Not used for merchants
        public ItemSlot[] Slots { get; } = new ItemSlot[24];
        IReadOnlyCollection<ItemSlot> IChest.Slots => Slots;

        public override string ToString()
        {
            var sb = new StringBuilder();
            if(Gold > 0)
            {
                sb.Append('$');
                sb.Append(Gold / 10);
                sb.Append('.');
                sb.Append(Gold % 10);
                sb.Append("0 ");
            }

            if(Rations > 0)
            {
                sb.Append(Rations);
                sb.Append(" food ");
            }

            bool first = true;
            foreach (var slot in Slots)
            {
                if (slot.Amount == 0)
                    continue;

                if (!first)
                    sb.Append(", ");
                sb.Append(slot);
                first = false;
            }

            return sb.ToString();
        }
    }
}
