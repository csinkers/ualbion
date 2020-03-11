namespace UAlbion.Formats.Assets
{
    public class SpellData
    {
        public SpellEnvironment Environment { get; set; }
        public byte Cost { get; set; }
        public byte LevelRequirement { get; set; }
        public SpellTarget Targets { get; set; }
        public byte Unk4 { get; set; }
    }
}
