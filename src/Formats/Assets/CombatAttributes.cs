namespace UAlbion.Formats.Assets
{
    public class CombatAttributes : ICombatAttributes
    {
        public int ExperiencePoints { get; set; }
        public ushort TrainingPoints { get; set; }
        public ushort LifePoints { get; set; }
        public ushort LifePointsMax { get; set; }
        public byte ActionPoints { get; set; }
        public ushort Protection { get; set; }
        public ushort Damage { get; set; }
        public PhysicalConditions PhysicalConditions { get; set; }
        public MentalConditions MentalConditions { get; set; }

        public CombatAttributes DeepClone() => (CombatAttributes) MemberwiseClone();
        public override string ToString() => $"XP:{ExperiencePoints} TP:{TrainingPoints} LP:{LifePoints}/{LifePointsMax} AP:{ActionPoints} D:{Damage} P:{Protection} PCond:{PhysicalConditions} MCond:{MentalConditions}";
    }
}
