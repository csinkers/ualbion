using System.Text;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace BuildTestingMaps;

public delegate int StringCreationFunc(string text);
public delegate string ScriptBuilderFunc(StringCreationFunc createString);
public abstract class MapBuilder
{
    protected BaseMapData Map { get; }
    readonly ListStringSet _mapStrings = [];
    readonly Dictionary<int, string> _scripts = [];

    public int AddMapText(string text) => _mapStrings.FindOrAdd(text);

    protected MapBuilder(BaseMapData map)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        while (Map.Npcs.Count < 96)
            Map.Npcs.Add(MapNpc.Unused);
    }

    public MapId Id => Map.Id;
    public int Width => Map.Width;
    public int Height => Map.Height;

    public MapBuilder SetChain(int i, ScriptBuilderFunc func)
    {
        _scripts[i] = func(AddMapText);
        return this;
    }

    public MapBuilder AddGlobalZone(TriggerTypes trigger, int chain)
    {
        if (chain is > ushort.MaxValue or < 0)
            throw new ArgumentOutOfRangeException(nameof(chain));
        Map.AddGlobalZone(trigger, (ushort)chain);
        return this;
    }

    public MapBuilder SetZone(byte x, byte y, TriggerTypes trigger, int chain)
    {
        if (chain is > ushort.MaxValue or < 0)
            throw new ArgumentOutOfRangeException(nameof(chain));
        Map.AddZone(x, y, trigger, (ushort)chain);
        return this;
    }

    public (BaseMapData, ListStringSet) Build()
    {
        var sb = new StringBuilder();
        foreach (var key in _scripts.Keys)
        {
            sb.AppendLine("{");
            sb.AppendLine("Chain" + key + ":");
            sb.AppendLine(_scripts[key]);
            sb.AppendLine("}");
        }

        var script = sb.ToString();
        var compiled = AlbionCompiler.Compile(script);

        foreach (var e in compiled.Events) Map.Events.Add(e);
        foreach (var c in compiled.Chains) Map.Chains.Add(c);

        Map.Unswizzle();
        return (Map, _mapStrings);
    }
}