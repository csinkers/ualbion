using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public interface IChest
    {
        ushort Gold { get; }
        ushort Rations { get; }
        IReadOnlyCollection<ItemSlot> Slots { get; }
    }
}
