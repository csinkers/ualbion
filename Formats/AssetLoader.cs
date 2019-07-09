using System;
using System.IO;

namespace UAlbion.Formats
{
    public enum SpriteType
    {
        Multi,
        FixedWidth,
        Bitmap,
    }

    public static class AssetLoader
    {
        public static object Load(BinaryReader br, AssetType type, long streamLength, string extensionHint)
        {
            switch (type)
            {
                case AssetType.MapData:
                case AssetType.LabData:
                case AssetType.Automap:
                    break;

                case AssetType.Font: return new AlbionFont(br, streamLength);

                case AssetType.Palette: // Known, TODO: Implement
                case AssetType.PaletteNull:
                    break;

                case AssetType.Picture:
                    return new AlbionSprite(br, streamLength, SpriteType.Bitmap);

                // Fixed size graphics
                case AssetType.Floor3D: return new AlbionSprite(br, streamLength, 64, 64); // Fixed width
                case AssetType.IconGraphics: return new AlbionSprite(br, streamLength, 16, 16); // Fixed width
                case AssetType.IconData: return new AlbionSprite(br, streamLength, 8, 8); // Weird little things... 8x8
                case AssetType.SmallPortrait: return new AlbionSprite(br, streamLength, 34, 37);
                case AssetType.TacticalIcon: return  new AlbionSprite(br, streamLength, 32, 48);
                case AssetType.ItemGraphics: return new AlbionSprite(br, streamLength, 16, 16);
                case AssetType.AutomapGraphics: return new AlbionSprite(br, streamLength, 8, 8);
                case AssetType.CombatBackground: return new AlbionSprite(br, streamLength, 360, 192);

                // Dependently sized graphics
                case AssetType.Wall3D: // Size varies, must be described elsewhere
                case AssetType.Object3D: // Described by LabData
                case AssetType.Overlay3D: // Size varies, must be described elsewhere
                    break;

                // Self describing graphics
                case AssetType.FullBodyPicture:
                case AssetType.BigPartyGraphics:
                case AssetType.SmallPartyGraphics:
                case AssetType.BigNpcGraphics:
                case AssetType.SmallNpcGraphics:
                case AssetType.MonsterGraphics:
                case AssetType.BackgroundGraphics: // Skyboxes
                case AssetType.CombatGraphics:
                    return new AlbionSprite(br, streamLength, SpriteType.Multi);

                // Textual resources
                case AssetType.EventTexts:
                case AssetType.MapTexts:
                case AssetType.Dictionary:
                    return new AlbionStringTable(br, streamLength);

                case AssetType.SystemTexts: return new AlbionStringTable(br, streamLength, StringTableType.SystemText); // Custom format, e.g. [numbers:format string]
                case AssetType.ItemNames: return new AlbionStringTable(br, streamLength, StringTableType.ItemNames);
                case AssetType.Script: return new AlbionScript(br, streamLength);

                case AssetType.Sample:
                case AssetType.WaveLibrary:
                    return new AlbionSample(br, streamLength);

                case AssetType.Song:

                case AssetType.ChestData:
                case AssetType.MerchantData:

                case AssetType.PartyCharacterData:
                case AssetType.MonsterCharacter:
                case AssetType.NpcCharacterData:

                case AssetType.BlockList:
                case AssetType.EventSet:
                case AssetType.ItemList:
                case AssetType.SpellData:
                case AssetType.MonsterGroup:
                case AssetType.Flic:
                case AssetType.Slab:
                case AssetType.TransparencyTables: // Bit weird, always 256 wide.
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}