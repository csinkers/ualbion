namespace UAlbion.Formats.Assets;

public interface IEffectiveCharacterSheet : ICharacterSheet
{
    int TotalWeight { get; }
    int MaxWeight { get; }
    int DisplayDamage { get; }
    int DisplayProtection { get; }
}