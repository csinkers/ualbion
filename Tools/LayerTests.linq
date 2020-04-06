<Query Kind="Program" />

const int RandomSetCount = 5000;
const int ParameterRange = 50;
const int MinimumScore = 73;
public const int TileY = 0;
public const int ReductionCycles = 1000;

void Main()
{
	var r = new Random();
	//Search(r);
	// */
	Parameters best = null;
	//* // Promising results:
	// best = new Parameters(4,1,1,2,10,1,2,4);
	best = new Parameters(3,1,3,1,2,8,1,2,3); // Works for pretty much everything except C+0 vs (N, O3) on OCU layering.
	// best = new Parameters(2, 1, 1, 1, 6, 1, 1, 2); // Works for pretty much everything but C+1 vs (L2, O3).
	// best = new Parameters(1, 10, 10, 10, 12, 0, 0, 10); // Works well outside with UOC layering.
	// best = new Parameters(16,1,2,4,34,1,2,14);
	for(int i = 0; i < 100; i++) best.Reduce(r);

	if (best != null)
	{
		var sb = new StringBuilder();
		(best, best.RunTests(sb), best.Sum).Dump();
		sb.ToString().Dump();
	}
	// */
}

void Search(Random r)
{
	var sets = new List<Parameters>();
	for (int i = 0; i < RandomSetCount * 1000; i++)
	{
		var p = new Parameters
		{
			YMultiplier = r.Next(5),
			CharacterOffset = r.Next(ParameterRange),
			L1 = r.Next(ParameterRange),
			L2 = r.Next(ParameterRange),
			L3 = r.Next(ParameterRange),
			O1 = r.Next(ParameterRange),
			O2 = r.Next(ParameterRange),
			O3 = r.Next(ParameterRange),
		};

		if (p.RunTests() < MinimumScore)
			continue;

		p.Reduce(r);
		sets.Add(p);

		if (sets.Count >= RandomSetCount)
			break;
	}

	var results = sets.Distinct().Select(x => (x, x.RunTests(), x.Sum)).OrderByDescending(x => x.Item2).ThenBy(x => x.Item3);
	var winners = results.Take(10);
	foreach (var winner in winners)
	{
		winner.x.Reduce(r);
		winner.Dump();
		var sb = new StringBuilder();
		winner.Item1.RunTests(sb);
		sb.ToString().Dump();
	}
}

public class TileData
{
	public TileLayer Layer; // Upper nibble of first byte
	public TileType Type; // Lower nibble of first byte
	public int GetDepth(Parameters p) => Type.ToInt(p) + Layer.ToInt(p);
}

public enum TileLayer : byte // Upper nibble of first byte
{
	Normal = 0, // Most floors, low and centre EW walls
	Layer1 = 2, // Mid EW walls, Overlay1
	Layer2 = 4, // Overlay2
	Layer3 = 6, // NS walls + Overlay3
	Unk8 = 8,
	Unk10 = 10,
	Unk12 = 12,
	Unk14 = 14,

	Unused1 = 1,
	Unused3 = 3,
	Unused5 = 5,
	Unused7 = 7,
	Unused9 = 9,
	Unused11 = 11,
	Unused13 = 13,
	Unused15 = 15,
}

public enum TileType : byte
{
	Normal = 0,   // Standard issue
	UnderlayIat = 1, // Underlay
	UnderlayCeltFloor = 2, // Underlay, celtic floors, toilet seats, random square next to pumps, top shelves of desks
	Overlay1 = 4, // Overlay
	Overlay2 = 5, // Overlay, only on continent maps?
	Overlay3 = 6, // Overlay
	Unk7 = 7,     // Overlay used on large plants on continent maps.
	Unk8 = 8,     // Underlay used on OkuloKamulos maps around fire
	Unk12 = 12,   // Overlay used on OkuloKamulos maps for covered fire / grilles
	Unk14 = 14,   // Overlay used on OkuloKamulos maps for open fire

	Unused3 = 3,
	Unused9 = 9,
	Unused10 = 10,
	Unused11 = 11,
	Unused13 = 13,
	Unused15 = 15,
}

public static class Extensions
{
	public static int ToInt(this TileType type, Parameters p)
	{
		int typeAdjust;
		switch ((int)type & 0x7)
		{
			case (int)TileType.Normal: typeAdjust = 0; break;
			case (int)TileType.Overlay1: typeAdjust = p.O1; break;
			case (int)TileType.Overlay2: typeAdjust = p.O2; break;
			case (int)TileType.Overlay3: typeAdjust = p.O3; break;
			// case (int)TileType.Unk7: typeAdjust = 3; break;
			default: typeAdjust = 0; break;
		};
		return typeAdjust;
	}

	public static int ToInt(this TileLayer layer, Parameters p)
	{
		int adjustment;
		switch ((int)layer & 0x7)
		{
			case (int)TileLayer.Normal: adjustment = 0; break;
			case (int)TileLayer.Layer1: adjustment = p.L1; break;
			case (int)TileLayer.Layer2: adjustment = p.L2; break;
			case (int)TileLayer.Layer3: adjustment = p.L3; break;
			default: adjustment = 0; break;
		};
		return adjustment;
	}
}

public class Parameters : IEquatable<Parameters>
{
	public int YMultiplier;
	public int CharacterOffset;
	public int OutdoorOffset;
	public int L1;
	public int L2;
	public int L3;
	public int O1;
	public int O2;
	public int O3;
	
	public Parameters() {}
	public Parameters(int mult, int charOff, int outdoors, int l1, int l2, int l3, int o1, int o2, int o3)
	{
		YMultiplier = mult;
		CharacterOffset = charOff;
		OutdoorOffset = outdoors;
		L1 = l1;
		L2 = l2;
		L3 = l3;
		O1 = o1;
		O2 = o2;
		O3 = o3;
	}

	int ShowResult(int x, int y, Func<int,int,bool> compare, StringBuilder sb, string note)
	{
		bool result = compare(x,y);
		if (sb != null)
		{
			if (result)
				sb.Append("Pass ");
			else
				sb.Append("==FAIL== ");

			if (note != null)
			{
				sb.Append("// ");
				sb.Append(note);
			}
			
			sb.AppendLine();
		}
		
		return result ? 1 : 0;
	}

	class DepthComparer
	{
		public Func<int, int, bool> Compare {get;set;}
		public string Name {get; set;}
	}
	int Test(int offset, TileLayer layer, TileType type, DepthComparer compare, StringBuilder sb, string note = null)
		=> TestCore(CharacterOffset, offset, layer, type, compare, sb, note);
	int TestOutdoor(int offset, TileLayer layer, TileType type, DepthComparer compare, StringBuilder sb, string note = null)
		=> TestCore(OutdoorOffset, offset, layer, type, compare, sb, note);

	int TestCore(int charBase, int offset, TileLayer layer, TileType type, DepthComparer compare, StringBuilder sb, string note = null)
	{
		var tile = new TileData
		{
			Layer = layer,
			Type = type
		};

		int characterLayer = charBase + YMultiplier * (offset + UserQuery.TileY);
		int tileLayer = tile.GetDepth(this) + YMultiplier * UserQuery.TileY;
		if (sb != null)
		{
			sb.Append($"(C={charBase}+{offset}) {characterLayer} ");
			sb.Append($"{compare.Name} ");
			sb.Append($"{tileLayer} ({layer} ({layer.ToInt(this)}) + {type} ({type.ToInt(this)})) : ");
		}
		
		return ShowResult(characterLayer, tileLayer, compare.Compare, sb, note);
	}

	int TestTiles(TileLayer layerX, TileType typeX, TileLayer layerY, TileType typeY, DepthComparer comparer, StringBuilder sb, string note = null)
	{
		var tx = new TileData
		{
			Layer = layerX,
			Type = typeX
		};
		var ty = new TileData
		{
			Layer = layerY,
			Type = typeY
		};
		int x = tx.GetDepth(this) + UserQuery.TileY;
		int y = ty.GetDepth(this) + UserQuery.TileY;

		if (sb != null)
		{
			sb.Append($"{layerX} ({layerX.ToInt(this)}) + {typeX} ({typeX.ToInt(this)}) = {x} ");
			sb.Append($"{comparer.Name} ");
			sb.Append($"{y} = {layerY} ({layerY.ToInt(this)}) + {typeY} ({typeY.ToInt(this)}) : ");
		}
		return ShowResult(x, y, comparer.Compare, sb, note);
	}

	public Parameters Clone() => (Parameters)MemberwiseClone();

	static DepthComparer DC(string name, Func<int, int, bool> comparer)
	 => new DepthComparer { Name = name, Compare = comparer };
	 
	static DepthComparer Less    = DC(" < ", (x, y) => x < y);
	static DepthComparer Greater = DC(" > ", (x, y) => x > y);
	static DepthComparer LE = DC("<=", (x, y) => x <= y);
	static DepthComparer GE = DC(">=", (x, y) => x >= y);

	public void Reduce(Random r)
	{
		int passing = RunTests();
		for (int i = 0; i < UserQuery.ReductionCycles; i++)
		{
			var knownGood = Clone();
			switch (r.Next(9))
			{
				case 0: if (YMultiplier > 0) YMultiplier--; break;
				case 1: if (CharacterOffset > 0) CharacterOffset--; break;
				case 2: if (OutdoorOffset > 0) OutdoorOffset--; break;
				case 3: if (L1 > 0) L1--; break;
				case 4: if (L2 > 0) L2--; break;
				case 5: if (L3 > 0) L3--; break;
				case 6: if (O1 > 0) O1--; break;
				case 7: if (O2 > 0) O2--; break;
				case 8: if (O3 > 0) O3--; break;
			}

			if (RunTests() < passing) // If it performs worse, revert the change
			{
				YMultiplier = knownGood.YMultiplier;
				CharacterOffset = knownGood.CharacterOffset;
				OutdoorOffset = knownGood.OutdoorOffset;
				L1 = knownGood.L1;
				L2 = knownGood.L2;
				L3 = knownGood.L3;
				O1 = knownGood.O1;
				O2 = knownGood.O2;
				O3 = knownGood.O3;
			}
		}
	}
	
	public bool Equals(Parameters other)
	{
		if(other == null) return false;
		return YMultiplier == other.YMultiplier &&
			CharacterOffset == other.CharacterOffset &&
			OutdoorOffset == other.OutdoorOffset &&
			L1 == other.L1 &&
			L2 == other.L2 &&
			L3 == other.L3 &&
			O1 == other.O1 &&
			O2 == other.O2 &&
			O3 == other.O3;
	}

	public override int GetHashCode() => (YMultiplier,CharacterOffset,OutdoorOffset,L1,L2,L3,O1,O2,O3).GetHashCode();

	public int Sum => YMultiplier + CharacterOffset + OutdoorOffset + L1 + L2 + L3 + O1 + O2 + O3;

	public int RunTests(StringBuilder sb = null)
	{
		int succeeded = 0;

		succeeded += Test(0, TileLayer.Normal, TileType.Normal, GE, sb);
		succeeded += Test(0, TileLayer.Normal, TileType.Overlay3, GE, sb, "Carpets on celtic levels. Troublesome");
		succeeded += Test(0, TileLayer.Layer1, TileType.Normal, LE, sb, "e.g. top edge of console in Tom's room");
		succeeded += Test(0, TileLayer.Layer2, TileType.Normal, Less, sb, "Iskai walls");
		succeeded += Test(0, TileLayer.Layer3, TileType.Normal, Less, sb, "Tom's bathroom door");

		succeeded += Test(0, TileLayer.Layer1, TileType.Overlay3, LE, sb);

		succeeded += Test(1, TileLayer.Normal, TileType.Overlay1, Greater, sb);
		succeeded += Test(1, TileLayer.Layer1, TileType.Overlay3, GE, sb, "Tom's door (floor tile overlays)");
		succeeded += Test(1, TileLayer.Layer2, TileType.Overlay1, Greater, sb);
		succeeded += Test(1, TileLayer.Layer2, TileType.Overlay3, LE, sb);
		succeeded += Test(1, TileLayer.Layer3, TileType.Normal, LE, sb, "Tom's bathroom door");

		succeeded += Test(2, TileLayer.Layer1, TileType.Overlay1, Greater, sb, "E chair in Tom's room");
		succeeded += Test(2, TileLayer.Layer3, TileType.Normal, LE, sb, "Tom's bathroom door");

		succeeded += TestOutdoor(0, TileLayer.Normal, TileType.Overlay3, GE, sb, "Outdoors overlays");
		succeeded += TestOutdoor(1, TileLayer.Layer2, TileType.Overlay3, Less, sb, "Large desert flower");

		succeeded += TestTiles(TileLayer.Layer1, TileType.Overlay1, TileLayer.Normal, TileType.Normal, Greater, sb);
		succeeded += TestTiles(TileLayer.Normal, TileType.Overlay3, TileLayer.Layer1, TileType.Normal, GE, sb);
		succeeded += TestTiles(TileLayer.Normal, TileType.Overlay3, TileLayer.Layer2, TileType.Normal, GE, sb, "Container on console in Tom's room");

		succeeded += 10 * TestTiles(TileLayer.Layer1, TileType.Normal, TileLayer.Normal, TileType.Normal, GE, sb);
		succeeded += 10 * TestTiles(TileLayer.Layer2, TileType.Normal, TileLayer.Layer1, TileType.Normal, GE, sb);
		succeeded += 10 * TestTiles(TileLayer.Layer3, TileType.Normal, TileLayer.Layer2, TileType.Normal, GE, sb);

		succeeded += 10 * TestTiles(TileLayer.Normal, TileType.Overlay1, TileLayer.Normal, TileType.Normal, GE, sb);
		succeeded += 10 * TestTiles(TileLayer.Normal, TileType.Overlay2, TileLayer.Normal, TileType.Overlay1, GE, sb);
		succeeded += 10 * TestTiles(TileLayer.Normal, TileType.Overlay3, TileLayer.Normal, TileType.Overlay2, GE, sb);
		return succeeded;
	}
}

