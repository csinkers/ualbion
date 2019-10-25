namespace UAlbion.Formats.Assets
{
    public interface ICharacterSheet
    {
        string Name { get; } // Debug name, not displayed to the player
        string EnglishName { get; }
        string GermanName { get; }
        string FrenchName { get; }

        CharacterType Type { get; }
        Gender Gender { get; }
        PlayerRace Race { get; }
        PlayerClass Class { get; }
        ushort Age { get; }
        byte Level { get; }
        uint ExperiencePoints { get; }
        ushort TrainingPoints { get; }

        PlayerLanguage Languages { get; }
        byte SpriteId { get; }
        byte PortraitId { get; }
        ushort EventSetId { get; }
        ushort WordSet { get; }

        // Combat
        ushort LifePoints { get; }
        ushort LifePointsMax { get; }
        byte ActionPoints { get; }
        ushort BaseProtection { get; }
        ushort BaseDamage { get; }
        PhysicalCondition PhysicalConditions { get; }
        MentalCondition MentalConditions { get; }

        // Grouped
        IMagicSkills Magic { get; }
        ICharacterInventory Inventory { get; }
        ICharacterAttributes Attributes { get; }
        ICharacterSkills Skills { get; }

        byte Unknown6 { get; }
        byte Unknown7 { get; }
        byte Unknown11 { get; }
        byte Unknown12 { get; }
        byte Unknown13 { get; }
        byte Unknown14 { get; }
        byte Unknown15 { get; }
        byte Unknown16 { get; }
        ushort Unknown1C { get; }
        ushort Unknown20 { get; }
        ushort Unknown22 { get; }

        ushort Unknown24 { get; }
        ushort Unknown26 { get; }
        ushort Unknown28 { get; }
        ushort Unknown2E { get; }
        ushort Unknown30 { get; }
        ushort Unknown36 { get; }
        ushort Unknown38 { get; }
        ushort Unknown3E { get; }
        ushort Unknown40 { get; }
        ushort Unknown46 { get; }
        ushort Unknown48 { get; }
        ushort Unknown4E { get; }
        ushort Unknown50 { get; }
        ushort Unknown56 { get; }
        ushort Unknown58 { get; }
        ushort Unknown5E { get; }
        ushort Unknown60 { get; }
        ushort Unknown66 { get; }
        ushort Unknown68 { get; }
        byte[] UnknownBlock6C { get; }
        ushort Unknown7E { get; }
        ushort Unknown80 { get; }
        ushort Unknown86 { get; }
        ushort Unknown88 { get; }
        ushort Unknown8E { get; }
        ushort Unknown90 { get; }
        byte[] UnknownBlock96 { get; }
        ushort UnknownCE { get; }
        ushort UnknownD6 { get; }
        byte[] UnknownBlockDA { get; }
        ushort UnknownFA { get; }
        ushort UnknownFC { get; }
    }
}