namespace UAlbion.Formats.Assets;

public class CombatAttributes : ICombatAttributes
{
    public int ExperiencePoints { get; set; }
    public ushort TrainingPoints { get; set; }
    ICharacterAttribute ICombatAttributes.LifePoints => LifePoints;
    public CharacterAttribute LifePoints { get; set; }
    public byte ActionPoints { get; set; }
    public ushort UnknownD6 { get; set; }
    public ushort UnknownD8 { get; set; }
    public Conditions Conditions { get; set; }

    public CombatAttributes DeepClone() => (CombatAttributes) MemberwiseClone();
    public override string ToString() => $"XP:{ExperiencePoints} TP:{TrainingPoints} LP:{LifePoints} AP:{ActionPoints} D:{UnknownD8} P:{UnknownD6} Cond:{Conditions}";
}