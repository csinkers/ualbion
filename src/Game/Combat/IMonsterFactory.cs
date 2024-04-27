using UAlbion.Formats.Ids;

namespace UAlbion.Game.Combat;

public interface IMonsterFactory
{
    Monster BuildMonster(MonsterId mobId, int position);
}