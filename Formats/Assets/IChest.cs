using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public interface IChest
    {
        ushort Gold { get; }
        ushort Rations { get; }
        IReadOnlyList<ItemSlot> Slots { get; }
    }
}
