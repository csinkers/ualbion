namespace UAlbion.Config
{
    public enum ContainerFormat
    {
        None, // Simple file containing a single asset.
        Xld, // Simple container, header contains file sizes, then followed by uncompressed raw file data.
        Directory, // Sub-assets are just files in a directory, named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
        Zip, // Zip compressed archive, sub-assets named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
        BinaryOffsets, // Read chunks from a binary file using offsets & lengths specified in the assets.json file.
        ItemList, // 0x3A bytes per item, no header.
        ItemNameList, // 20 bytes per name, each item has 3 names (German, English, French)
        SpellList, // 5 bytes per spell, 30 spells per class, 7 classes. No header.
    }
}
