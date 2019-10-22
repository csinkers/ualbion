namespace UAlbion.Formats.Assets
{
    public interface ICharacterSheet
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
}