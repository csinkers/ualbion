namespace UAlbion.Formats.Assets;

public interface ICombatAttributes
{
    int ExperiencePoints { get; }
    ushort TrainingPoints { get; }
    ICharacterAttribute LifePoints { get; }
    byte ActionPoints { get; }
    ushort UnknownD6 { get; }
    ushort UnknownD8 { get; }
    PlayerConditions Conditions { get; }
}