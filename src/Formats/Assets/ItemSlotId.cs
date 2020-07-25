using System;

namespace UAlbion.Formats.Assets
{
    public enum ItemSlotId : byte // Enum values that are more convenient for in-memory use (item slot array accessing etc)
    {
        Slot0 = 0,
        NormalSlotCount = 24,
        Gold = 24,
        Rations = 25,
        ChestSlotCount = 26,
        Neck = 26,
        Head,
        Tail,
        RightHand,
        Chest,
        LeftHand,
        RightFinger,
        Feet,
        LeftFinger,
        RightHandOrTail,
        CharacterSlotCount,

        // Dummy slot id used for the character's full body
        // picture as a background on the middle inventory pane.
        CharacterBody = 0xfc, 

        None = 0xff,
    }

    enum PersistedItemSlotId : byte // Enum values used by the actual game data files
    {
        None = 0,
        Neck = 1,
        Head = 2,
        Tail = 3,
        RightHand = 4,
        Chest = 5,
        LeftHand = 6,
        RightFinger = 7,
        Feet = 8,
        LeftFinger = 9,
        RightHandOrTail = 10,
        Gold = 11,
        Rations = 12,
        CharacterBody = 13,
        Slot0 = 14,
    }

    public static class ItemSlotIdExtensions
    {
        internal static PersistedItemSlotId ToPersisted(this ItemSlotId id) =>
            id switch
            {
                ItemSlotId.Slot0 => PersistedItemSlotId.Slot0,
                ItemSlotId.Neck => PersistedItemSlotId.Neck,
                ItemSlotId.Head => PersistedItemSlotId.Head,
                ItemSlotId.Tail => PersistedItemSlotId.Tail,
                ItemSlotId.RightHand => PersistedItemSlotId.RightHand,
                ItemSlotId.Chest => PersistedItemSlotId.Chest,
                ItemSlotId.LeftHand => PersistedItemSlotId.LeftHand,
                ItemSlotId.RightFinger => PersistedItemSlotId.RightFinger,
                ItemSlotId.Feet => PersistedItemSlotId.Feet,
                ItemSlotId.LeftFinger => PersistedItemSlotId.LeftFinger,
                ItemSlotId.RightHandOrTail => PersistedItemSlotId.RightHandOrTail,
                ItemSlotId.CharacterBody => PersistedItemSlotId.CharacterBody,
                ItemSlotId.None => PersistedItemSlotId.None,
                _ => (PersistedItemSlotId)((int)id - (int)ItemSlotId.Slot0 + (int)PersistedItemSlotId.Slot0)
            };

        internal static ItemSlotId ToMemory(this PersistedItemSlotId persisted)
            => persisted switch
            {
                PersistedItemSlotId.None            => ItemSlotId.None,
                PersistedItemSlotId.Neck            => ItemSlotId.Neck,
                PersistedItemSlotId.Head            => ItemSlotId.Head,
                PersistedItemSlotId.Tail            => ItemSlotId.Tail,
                PersistedItemSlotId.RightHand       => ItemSlotId.RightHand,
                PersistedItemSlotId.Chest           => ItemSlotId.Chest,
                PersistedItemSlotId.LeftHand        => ItemSlotId.LeftHand,
                PersistedItemSlotId.RightFinger     => ItemSlotId.RightFinger,
                PersistedItemSlotId.Feet            => ItemSlotId.Feet,
                PersistedItemSlotId.LeftFinger      => ItemSlotId.LeftFinger,
                PersistedItemSlotId.RightHandOrTail => ItemSlotId.RightHandOrTail,
                PersistedItemSlotId.Gold            => ItemSlotId.Gold,
                PersistedItemSlotId.Rations         => ItemSlotId.Rations,
                PersistedItemSlotId.CharacterBody   => ItemSlotId.CharacterBody,
                PersistedItemSlotId.Slot0           => ItemSlotId.Slot0,
                _ => throw new ArgumentOutOfRangeException(nameof(persisted), persisted, null)
            };

        public static bool IsBodyPart(this ItemSlotId id)
        {
            return id switch
            {
                { } x when
                    x == ItemSlotId.Neck ||
                    x == ItemSlotId.Head ||
                    x == ItemSlotId.Tail ||
                    x == ItemSlotId.RightHand ||
                    x == ItemSlotId.Chest ||
                    x == ItemSlotId.LeftHand ||
                    x == ItemSlotId.RightFinger ||
                    x == ItemSlotId.Feet ||
                    x == ItemSlotId.LeftFinger ||
                    x == ItemSlotId.RightHandOrTail ||
                    x == ItemSlotId.CharacterBody => true,
                _ => false
            };
        }

        public static bool IsSpecial(this ItemSlotId id) => id == ItemSlotId.Gold || id == ItemSlotId.Rations;
    }
}
