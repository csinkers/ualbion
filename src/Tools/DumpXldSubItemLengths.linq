<Query Kind="Program">
  <NuGetReference>SerdesNet</NuGetReference>
  <Namespace>SerdesNet</Namespace>
</Query>

const string XldDir = @"C:\Depot\bb\ualbion\albion\CD\XLDLIBS";
const string ResultPath = @"C:\Depot\bb\ualbion\data\exported\lengths.txt";
const string MagicString = "XLD0I";

void Main()
{
	var files = Directory.EnumerateFiles(XldDir, "*.xld", SearchOption.AllDirectories);
	var sb = new StringBuilder();
	foreach(var file in files)
	{
		sb.AppendLine(file);
		using var stream = File.OpenRead(file);
		using var br = new BinaryReader(stream);
		using var s = new GenericBinaryReader(br, stream.Length, Encoding.ASCII.GetString);
		var lengths = LoadXld(s);
		for(int i = 0; i < lengths.Length; i++)
			sb.AppendLine($"    {i} = {lengths[i]}");
	}
	File.WriteAllText(ResultPath, sb.ToString());
}

int[] LoadXld(ISerializer s)
{
	string magic = s.NullTerminatedString("MagicString", MagicString);
	s.Check();
	if (magic != MagicString)
		throw new FormatException("XLD file magic string not found");

	ushort objectCount = s.UInt16("ObjectCount", 0);
	s.Check();
	int[] lengths = new int[objectCount];

	for (int i = 0; i < objectCount; i++)
	{
		lengths[i] = s.Int32("Length" + i, lengths[i]);
		s.Check();
	}

	s.End();
	return lengths;
}