using System;
using System.IO;
using SixLabors.ImageSharp;
using UAlbion.Formats;

namespace UAlbion.Game
{
    public static class AssetLoader
    {
        public static object Load(BinaryReader br, AssetType type, int id, string name, int streamLength, object context)
        {
            switch (type)
            {
                case AssetType.MapData:
                case AssetType.LabData:
                case AssetType.Automap:
                    break;

                case AssetType.Font: return new AlbionFont(br, streamLength);
                case AssetType.Palette: return new AlbionPalette(name, br, streamLength, (AlbionPalette.PaletteContext)context);
                case AssetType.PaletteNull: return br.ReadBytes(streamLength);
                case AssetType.Picture: return Image.Load(br.BaseStream);

                // Fixed size graphics
                case AssetType.Floor3D: return new AlbionSprite(br, streamLength, 64, 64, name); // Fixed width
                case AssetType.IconGraphics: return new AlbionSprite(br, streamLength, 16, 16, name); // Fixed width
                case AssetType.IconData: return new AlbionSprite(br, streamLength, 8, 8, name); // Weird little things... 8x8
                case AssetType.SmallPortrait: return new AlbionSprite(br, streamLength, 34, 37, name);
                case AssetType.TacticalIcon: return  new AlbionSprite(br, streamLength, 32, 48, name);
                case AssetType.ItemGraphics: return new AlbionSprite(br, streamLength, 16, 16, name);
                case AssetType.AutomapGraphics: return new AlbionSprite(br, streamLength, 8, 8, name);
                case AssetType.CombatBackground: return new AlbionSprite(br, streamLength, 360, 192, name);

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
                    return new AlbionSprite(br, streamLength, name);

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