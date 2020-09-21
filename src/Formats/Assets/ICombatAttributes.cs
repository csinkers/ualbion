namespace UAlbion.Formats.Assets
{
    public interface ICombatAttributes
    {
        int ExperiencePoints { get; }
        ushort TrainingPoints { get; }
        ushort LifePoints { get; }
        ushort LifePointsMax { get; }
        byte ActionPoints { get; }
        ushort Protection { get; }
        ushort Damage { get; }
        PhysicalConditions PhysicalConditions { get; }
        MentalConditions MentalConditions { get; }
    }
}
