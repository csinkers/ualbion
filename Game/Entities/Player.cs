namespace UAlbion.Game.Entities
{
    class PlayerStats
    {
        int _level;

        int _lifePoints;
        int _lifePointsMax;
        int _spellPoints;
        int _spellPointsMax;
        int _experiencePoints;
        int _trainingPoints;

        int _strength;
        int _intelligence;
        int _dexterity;
        int _speed;
        int _stamina;
        int _luck;
        int _magicResistance;
        int _magicTalent;

        int _closeCombat;
        int _rangedCombat;
        int _criticalChance;
        int _lockPicking;

        int _strengthMax;
        int _intelligenceMax;
        int _dexterityMax;
        int _speedMax;
        int _staminaMax;
        int _luckMax;
        int _magicResistanceMax;
        int _magicTalentMax;

        int _closeCombatMax;
        int _rangedCombatMax;
        int _criticalChanceMax;
        int _lockPickingMax;
    }

    internal class Player
    {
        string _name;
        int _age;
        bool _isMale;
        int _combatPosition;
        PlayerStats _stats;

        Item[] _inventory = new Item[24]; // 4x6
        Item _head;
        Item _neck;
        Item _leftHand;
        Item _rightHand;
        Item _leftRing;
        Item _rightRing;
        Item _feet;
        Item _torso;
    }
}
