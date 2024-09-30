namespace UAlbion.Formats.Assets;

public interface ICombatAttributes
{
    int ExperiencePoints { get; }
    ushort TrainingPoints { get; }
    ICharacterAttribute LifePoints { get; }
    byte ActionPoints { get; }
    ushort BaseDefense { get; }
    ushort BonusDefense { get; }
    PlayerConditions Conditions { get; }
}