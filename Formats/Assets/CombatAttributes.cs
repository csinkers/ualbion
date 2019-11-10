namespace UAlbion.Formats.Assets
{
    public class CombatAttributes : ICombatAttributes
    {
        public uint ExperiencePoints { get; set; }
        public ushort TrainingPoints { get; set; }
        public ushort LifePoints { get; set; }
        public ushort LifePointsMax { get; set; }
        public byte ActionPoints { get; set; }
        public ushort BaseProtection { get; set; }
        public ushort BaseDamage { get; set; }
        public PhysicalCondition PhysicalConditions { get; set; }
        public MentalCondition MentalConditions { get; set; }
    }
}