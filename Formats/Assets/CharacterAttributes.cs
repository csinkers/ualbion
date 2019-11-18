namespace UAlbion.Formats.Assets
{
    public interface ICharacterAttributes
    {
        ushort Strength { get; }
        ushort Intelligence { get; }
        ushort Dexterity { get; }
        ushort Speed { get; }
        ushort Stamina { get; }
        ushort Luck { get; }
        ushort MagicResistance { get; }
        ushort MagicTalent { get; }

        ushort StrengthMax { get; }
        ushort IntelligenceMax { get; }
        ushort DexterityMax { get; }
        ushort SpeedMax { get; }
        ushort StaminaMax { get; }
        ushort LuckMax { get; }
        ushort MagicResistanceMax { get; }
        ushort MagicTalentMax { get; }
    }

    public class CharacterAttributes : ICharacterAttributes
    {
        public override string ToString() => 
            $"S{Strength}/{StrengthMax} I{Intelligence}/{IntelligenceMax} D{Dexterity}/{DexterityMax} " +
            $"Sp{Speed}/{SpeedMax} St{Stamina}/{StaminaMax} L{Luck}/{LuckMax} " + 
            $"MR{MagicResistance}/{MagicResistanceMax} MT{MagicTalent}/{MagicTalentMax}";

        public ushort Strength { get; set; }
        public ushort Intelligence { get; set; }
        public ushort Dexterity { get; set; }
        public ushort Speed { get; set; }
        public ushort Stamina { get; set; }
        public ushort Luck { get; set; }
        public ushort MagicResistance { get; set; }
        public ushort MagicTalent { get; set; }

        public ushort StrengthMax { get; set; }
        public ushort IntelligenceMax { get; set; }
        public ushort DexterityMax { get; set; }
        public ushort SpeedMax { get; set; }
        public ushort StaminaMax { get; set; }
        public ushort LuckMax { get; set; }
        public ushort MagicResistanceMax { get; set; }
        public ushort MagicTalentMax { get; set; }
        public CharacterAttributes DeepClone() => (CharacterAttributes)MemberwiseClone();
    }
}