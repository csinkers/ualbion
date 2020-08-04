using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class MonsterGroup
    {
        // 36 bytes = 2 bytes/slot, 6x3 combat grid.
        public MonsterCharacterId?[] Grid { get; } = new MonsterCharacterId?[6 * 3];

        public static MonsterGroup Serdes(int _, MonsterGroup m, ISerializer s)
        {
            m ??= new MonsterGroup();
            for(int i = 0; i < m.Grid.Length; i++)
            {
                m.Grid[i] = s.TransformEnumU8(
                    i.ToString(),
                    m.Grid[i],
                    StoreIncrementedNullZero<MonsterCharacterId>.Instance);
                s.UInt8(null, 0);
            }
            return m;
        }
    }
}
