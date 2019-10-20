namespace UAlbion.Game.State
{
    public class ICharacterSheet
    {
        int Level { get; }

        int LifePoints { get; }
        int LifePointsMax { get; }
        int SpellPoints { get; }
        int SpellPointsMax { get; }
        int ExperiencePoints { get; }
        int TrainingPoints { get; }

        int Strength { get; }
        int Intelligence { get; }
        int Dexterity { get; }
        int Speed { get; }
        int Stamina { get; }
        int Luck { get; }
        int MagicResistance { get; }
        int MagicTalent { get; }

        int CloseCombat { get; }
        int RangedCombat { get; }
        int CriticalChance { get; }
        int LockPicking { get; }

        int StrengthMax { get; }
        int IntelligenceMax { get; }
        int DexterityMax { get; }
        int SpeedMax { get; }
        int StaminaMax { get; }
        int LuckMax { get; }
        int MagicResistanceMax { get; }
        int MagicTalentMax { get; }

        int CloseCombatMax { get; }
        int RangedCombatMax { get; }
        int CriticalChanceMax { get; }
        int LockPickingMax { get; }
    }

    public class CharacterSheet : ICharacterSheet
    {
        public int Level { get; set; }

        public int LifePoints { get; set; }
        public int LifePointsMax { get; set; }
        public int SpellPoints { get; set; }
        public int SpellPointsMax { get; set; }
        public int ExperiencePoints { get; set; }
        public int TrainingPoints { get; set; }

        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
        public int Speed { get; set; }
        public int Stamina { get; set; }
        public int Luck { get; set; }
        public int MagicResistance { get; set; }
        public int MagicTalent { get; set; }

        public int CloseCombat { get; set; }
        public int RangedCombat { get; set; }
        public int CriticalChance { get; set; }
        public int LockPicking { get; set; }

        public int StrengthMax { get; set; }
        public int IntelligenceMax { get; set; }
        public int DexterityMax { get; set; }
        public int SpeedMax { get; set; }
        public int StaminaMax { get; set; }
        public int LuckMax { get; set; }
        public int MagicResistanceMax { get; set; }
        public int MagicTalentMax { get; set; }

        public int CloseCombatMax { get; set; }
        public int RangedCombatMax { get; set; }
        public int CriticalChanceMax { get; set; }
        public int LockPickingMax { get; set; }
    }
}