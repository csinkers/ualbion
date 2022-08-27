using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

public enum ActionType : byte
{
    Word = 0x0,
    AskAboutItem = 0x1, // byte-param is ItemType?
    Unk2 = 0x2, // Pay money? See ES156 (Garris, Gratogel sailor)
    Unk3 = 0x3, // Unused?
    AskToJoin = 0x4, // Konny
    AskToLeave = 0x5,
    StartDialogue = 0x6,
    FinishDialogue = 0x7,
    DialogueLine = 0x8,
    Unk9 = 0x9, // 234, Riko, 242, Gerwad
    UnkA = 0xA,
    UnkB = 0xB,
    UnkC = 0xC,
    UnkD = 0xD,
    UnkE = 0xE, // 981_Tom, endgame related
    UnkF = 0xF,
    Unk10 = 0x10,
    Unk11 = 0x11,
    Unk12 = 0x12,
    Unk13 = 0x13,
    Unk14 = 0x14,
    Unk15 = 0x15,
    Unk16 = 0x16,
    Unk17 = 0x17, // Sira, some kind of spellcasting
    Unk18 = 0x18,
    Unk19 = 0x19,
    Unk1A = 0x1A,
    Unk1B = 0x1B,
    Unk1C = 0x1C,
    Unk1D = 0x1D,
    Unk1E = 0x1E,
    Unk1F = 0x1F,
    Unk20 = 0x20,
    Unk21 = 0x21,
    Unk22 = 0x22,
    Unk23 = 0x23,
    Unk24 = 0x24,
    Unk25 = 0x25,
    Unk26 = 0x26,
    Unk27 = 0x27,
    Unk28 = 0x28,
    Unk29 = 0x29,
    Unk2A = 0x2A,
    Unk2B = 0x2B,
    Unk2C = 0x2C,
    Unk2D = 0x2D, // Sira, some kind of spellcasting
    UseItem = 0x2E,
    EquipItem = 0x2F,
    UnequipItem = 0x30,
    Unk31 = 0x31,
    Unk32 = 0x32,
    Unk33 = 0x33,
    PlacedItemInChest = 0x34, // Item related, used for detecting when Tom has put the pistol in the wall locker
    Unk35 = 0x35,
    DropItem = 0x36,
    Unk37 = 0x37,
    Unk38 = 0x38,
    SignalTarget = 0x39,
    Unk3A = 0x3A,
    Unk3B = 0x3B,
    Unk3C = 0x3C,
    PartySleeps = 0x3D // Used to trigger Sira + Mellthas script
}

public static class ActionTypeExtensions
{
    public static AssetType GetAssetType(this ActionType actionType) =>
        actionType switch
        {
            ActionType.StartDialogue 
         or ActionType.AskToLeave
         or ActionType.FinishDialogue 
                => AssetType.None,

            ActionType.AskAboutItem
         or ActionType.UseItem
         or ActionType.EquipItem
         or ActionType.UnequipItem
         or ActionType.DropItem 
         or ActionType.PlacedItemInChest
                => AssetType.Item,

            ActionType.DialogueLine => AssetType.PromptNumber,
            ActionType.Word => AssetType.Word,
            _ => AssetType.Unknown
        };
}
