using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class InterpolatedItemSlot : IReadOnlyItemSlot
    {
        readonly Func<IReadOnlyItemSlot> _a;
        readonly Func<IReadOnlyItemSlot> _b;
        readonly Func<float> _getLerp;

        public InterpolatedItemSlot(Func<IReadOnlyItemSlot> a, Func<IReadOnlyItemSlot> b, Func<float> getLerp)
        {
            _a = a;
            _b = b;
            _getLerp = getLerp;
        }

        public ushort Amount => (ushort)ApiUtil.Lerp(_a().Amount, _b().Amount, _getLerp());
        public byte Charges => _b().Charges;
        public byte Enchantment => _b().Enchantment;
        public ItemSlotFlags Flags => _b().Flags;
        public IContents Item => _b().Item;
        public Vector2 LastUiPosition => _b().LastUiPosition;
    }
}