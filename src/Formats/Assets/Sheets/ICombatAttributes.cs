namespace UAlbion.Formats.Assets.Sheets;

public interface ICombatAttributes
{
    int ExperiencePoints { get; }
    ushort TrainingPoints { get; }
    ICharacterAttribute LifePoints { get; }
    byte ActionPoints { get; }
    ushort BaseDefense { get; }
    short BonusDefense { get; }
    ushort BaseAttack { get; }
    short BonusAttack { get; }
    PlayerConditions Conditions { get; }
}