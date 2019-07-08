using System.Collections.Generic;

namespace UAlbion.Formats
{
    public enum AssetType
    {
        MapData,
        IconData,
        IconGraphics,
        Palette,
        PaletteNull,
        Slab,
        BigPartyGraphics,
        SmallPartyGraphics,
        LabData,
        Wall3D,
        Object3D,
        Overlay3D,
        Floor3D,
        BigNpcGraphics,
        BackgroundGraphics,
        Font,
        BlockList,
        PartyCharacterData,
        SmallPortrait,
        SystemTexts,
        EventSet,
        EventTexts,
        MapTexts,
        ItemList,
        ItemNames,
        ItemGraphics,
        FullBodyPicture,
        Automap,
        AutomapGraphics,
        Song,
        Sample,
        WaveLibrary,
        Unnamed2, // Unused
        ChestData,
        MerchantData,
        NpcCharacterData,
        MonsterGroup,
        MonsterCharacter,
        MonsterGraphics,
        CombatBackground,
        CombatGraphics,
        TacticalIcon,
        SpellData,
        SmallNpcGraphics,
        Flic,
        Dictionary,
        Script,
        Picture,
        TransparencyTables,
    }

    enum AssetLocation
    {
        Base,
        Localised,
        Initial
    }

    struct AssetId
    {
        // ReSharper disable StringLiteralTypo
        static IDictionary<AssetType, (AssetLocation, string)> _assetFiles = new Dictionary<AssetType, (AssetLocation, string)> {
            { AssetType.MapData,            (AssetLocation.Base,      "MAPDATA0.XLD") },
            { AssetType.IconData,           (AssetLocation.Base,      "ICONDAT0.XLD") },
            { AssetType.IconGraphics,       (AssetLocation.Base,      "ICONGFX0.XLD") },
            { AssetType.Palette,            (AssetLocation.Base,      "PALETTE0.XLD") },
            { AssetType.PaletteNull,        (AssetLocation.Base,      "PALETTE.000" ) },
            { AssetType.Slab,               (AssetLocation.Base,      "SLAB"        ) },
            { AssetType.BigPartyGraphics,   (AssetLocation.Base,      "PARTGR0.XLD" ) },
            { AssetType.SmallPartyGraphics, (AssetLocation.Base,      "PARTKL0.XLD" ) },
            { AssetType.LabData,            (AssetLocation.Base,      "LABDATA0.XLD") },
            { AssetType.Wall3D,             (AssetLocation.Base,      "3DWALLS0.XLD") },
            { AssetType.Object3D,           (AssetLocation.Base,      "3DOBJEC0.XLD") },
            { AssetType.Overlay3D,          (AssetLocation.Base,      "3DOVERL0.XLD") },
            { AssetType.Floor3D,            (AssetLocation.Base,      "3DFLOOR0.XLD") },
            { AssetType.BigNpcGraphics,     (AssetLocation.Base,      "NPCGR0.XLD"  ) },
            { AssetType.BackgroundGraphics, (AssetLocation.Base,      "3DBCKGR0.XLD") },
            { AssetType.Font,               (AssetLocation.Base,      "FONTS0.XLD"  ) },
            { AssetType.BlockList,          (AssetLocation.Base,      "BLKLIST0.XLD") },
            { AssetType.PartyCharacterData, (AssetLocation.Initial,   "PRTCHAR0.XLD") },
            { AssetType.SmallPortrait,      (AssetLocation.Base,      "SMLPORT0.XLD") },
            { AssetType.SystemTexts,        (AssetLocation.Localised, "SYSTEXTS"    ) },
            { AssetType.EventSet,           (AssetLocation.Base,      "EVNTSET0.XLD") },
            { AssetType.EventTexts,         (AssetLocation.Localised, "EVNTTXT0.XLD") },
            { AssetType.MapTexts,           (AssetLocation.Localised, "MAPTEXT0.XLD") },
            { AssetType.ItemList,           (AssetLocation.Base,      "ITEMLIST.DAT") },
            { AssetType.ItemNames,          (AssetLocation.Base,      "ITEMNAME.DAT") },
            { AssetType.ItemGraphics,       (AssetLocation.Base,      "ITEMGFX"     ) },
            { AssetType.FullBodyPicture,    (AssetLocation.Base,      "FBODPIX0.XLD") },
            { AssetType.Automap,            (AssetLocation.Initial,   "AUTOMAP0.XLD") },
            { AssetType.AutomapGraphics,    (AssetLocation.Base,      "AUTOGFX0.XLD") },
            { AssetType.Song,               (AssetLocation.Base,      "SONGS0.XLD"  ) },
            { AssetType.Sample,             (AssetLocation.Base,      "SAMPLES0.XLD") },
            { AssetType.WaveLibrary,        (AssetLocation.Base,      "WAVELIB0.XLD") },
            // { AssetType.Unnamed2,        (AssetLocation.Base,      ""         ) },
            { AssetType.ChestData,          (AssetLocation.Initial,   "CHESTDT0.XLD") },
            { AssetType.MerchantData,       (AssetLocation.Initial,   "MERCHDT0.XLD") },
            { AssetType.NpcCharacterData,   (AssetLocation.Initial,   "NPCCHAR0.XLD") },
            { AssetType.MonsterGroup,       (AssetLocation.Base,      "MONGRP0.XLD" ) },
            { AssetType.MonsterCharacter,   (AssetLocation.Base,      "MONCHAR0.XLD") },
            { AssetType.MonsterGraphics,    (AssetLocation.Base,      "MONGFX0.XLD" ) },
            { AssetType.CombatBackground,   (AssetLocation.Base,      "COMBACK0.XLD") },
            { AssetType.CombatGraphics,     (AssetLocation.Base,      "COMGFX0.XLD" ) },
            { AssetType.TacticalIcon,       (AssetLocation.Base,      "TACTICO0.XLD") },
            { AssetType.SpellData,          (AssetLocation.Base,      "SPELLDAT.DAT") },
            { AssetType.SmallNpcGraphics,   (AssetLocation.Base,      "NPCKL0.XLD"  ) },
            { AssetType.Flic,               (AssetLocation.Localised, "FLICS0.XLD"  ) },
            { AssetType.Dictionary,         (AssetLocation.Localised, "WORDLIS0.XLD") },
            { AssetType.Script,             (AssetLocation.Base,      "SCRIPT0.XLD" ) },
            { AssetType.Picture,            (AssetLocation.Base,      "PICTURE0.XLD") },
            { AssetType.TransparencyTables, (AssetLocation.Base,      "TRANSTB0.XLD") }
        };
        // ReSharper restore StringLiteralTypo

        AssetType _type;
        int _id; // id/100 = file, id%100 = resource inside file.
    }
}
