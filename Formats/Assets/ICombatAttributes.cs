namespace UAlbion.Formats.Assets
{
    public interface ICombatAttributes
    {
        uint ExperiencePoints { get; }
        ushort TrainingPoints { get; }
        ushort LifePoints { get; }
        ushort LifePointsMax { get; }
        byte ActionPoints { get; }
        ushort BaseProtection { get; }
        ushort BaseDamage { get; }
        PhysicalCondition PhysicalConditions { get; }
        MentalCondition MentalConditions { get; }
    }
}