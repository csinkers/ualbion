namespace UAlbion.Formats.Assets
{
    public interface ICombatAttributes
    {
        uint ExperiencePoints { get; }
        ushort TrainingPoints { get; }
        ushort LifePoints { get; }
        ushort LifePointsMax { get; }
        byte ActionPoints { get; }
        ushort Protection { get; }
        ushort Damage { get; }
        PhysicalCondition PhysicalConditions { get; }
        MentalCondition MentalConditions { get; }
    }
}