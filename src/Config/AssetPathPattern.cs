using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UAlbion.Config;

public class AssetPathPattern
{
    static readonly object SyncRoot = new();
    static readonly Dictionary<string, AssetPathPattern> Cache = new();
    readonly string _pattern;
    readonly Regex _regex;
    readonly List<Part> _parts = new();

    enum PartType
    {
        Id,
        Text,
        SubAsset,
        Name,
        Palette,
        PaletteFrame,
        IgnoreNum,
    }

    readonly struct Part
    {
        public override string ToString() => 
            Type == PartType.Text 
                ? $"\"{Value}\"" 
                : $"{Type}{(Value==null?"":" ")}{Value}";

        public Part(string value) { Type = PartType.Text; Value = value; }
        public Part(string name, string value)
        {
            Type =
                name.ToUpperInvariant() switch
                {
                    "0" => PartType.Id,
                    "ID" => PartType.Id,

                    "1" => PartType.SubAsset,
                    "FRAME" => PartType.SubAsset,
                    "SUBASSET" => PartType.SubAsset,

                    "2" => PartType.Name,
                    "NAME" => PartType.Name,

                    "3" => PartType.Palette,
                    "P" => PartType.Palette,
                    "PAL" => PartType.Palette,
                    "PALETTE" => PartType.Palette,

                    "4" => PartType.PaletteFrame,
                    "PF" => PartType.PaletteFrame,
                    "PALFRAME" => PartType.PaletteFrame,
                    
                    "IGNORENUM" => PartType.IgnoreNum,
                    _ => throw new FormatException($"Tried to parse unknown asset path pattern component \"{name}\"")
                };

            Value = value;
        }
        public PartType Type { get; }
        public string Value { get; }
    }

    public bool IsEmpty => _parts.Count == 0;
    public override string ToString() => _pattern;

    public static AssetPathPattern Build(string pattern)
    {
        lock (SyncRoot)
        {
            if (!Cache.TryGetValue(pattern, out var result))
            {
                result = new AssetPathPattern(pattern);
                Cache[pattern] = result;
            }

            return result;
        }
    }

    AssetPathPattern(string pattern)
    {
        _pattern = pattern;
        if (string.IsNullOrEmpty(pattern))
            return;

        var sb = new StringBuilder();
        string argName = null;
        bool inParam = false;
        foreach (var c in pattern)
        {
            switch (c)
            {
                case '{':
                    if (inParam)
                        throw new FormatException("Unmatched {");

                    if (sb.Length > 0)
                    {
                        _parts.Add(new Part(sb.ToString()));
                        sb.Clear();
                    }

                    inParam = true;
                    break;

                case '}':
                    if (!inParam)
                        throw new FormatException("Unmatched }");

                    inParam = false;
                    _parts.Add(new Part(argName ?? sb.ToString(), argName == null ? null : sb.ToString()));
                    sb.Clear();
                    argName = null;
                    break;

                case ':':
                    if (inParam)
                    {
                        argName = sb.ToString();
                        sb.Clear();
                    }
                    else sb.Append(':');
                    break;

                default:
                    sb.Append(c);
                    break;
            }
        }

        if (sb.Length > 0)
            _parts.Add(new Part(sb.ToString()));

        sb.Clear();
        foreach (var part in _parts)
        {
            switch (part.Type)
            {
                case PartType.Id:           sb.Append(@"(?<Id>\d+)"); break;
                case PartType.Text:         sb.Append(Regex.Escape(part.Value)); break;
                case PartType.SubAsset:     sb.Append(@"(?<SubAsset>\d+)"); break;
                case PartType.Name:         sb.Append(@"(?<Name>\w+)"); break;
                case PartType.Palette:      sb.Append(@"(?<Palette>\d+)"); break;
                case PartType.PaletteFrame: sb.Append(@"(?<PFrame>\d+)"); break;
                case PartType.IgnoreNum:    sb.Append(@"\d+"); break;
            }
        }

        _regex = new Regex(sb.ToString());
    }

    static string FormatInt(int v, string format) =>
        format != null
            ? v.ToString(format, CultureInfo.InvariantCulture)
            : v.ToString(CultureInfo.InvariantCulture);

    public string Format(in AssetPath path)
    {
        var sb = new StringBuilder();
        foreach (var part in _parts)
        {
            switch (part.Type)
            {
                case PartType.Id: sb.Append(FormatInt(path.AssetId.Id, part.Value)); break;
                case PartType.SubAsset: sb.Append(FormatInt(path.SubAsset, part.Value)); break;
                case PartType.Text: sb.Append(part.Value); break;
                case PartType.Name: sb.Append(path.Name); break;
                case PartType.IgnoreNum: sb.Append('0'); break;
                case PartType.Palette:
                    if (path.PaletteId.HasValue)
                        sb.Append(FormatInt(path.PaletteId.Value, part.Value));
                    break;
                case PartType.PaletteFrame:
                    if (path.PaletteFrame.HasValue)
                        sb.Append(FormatInt(path.PaletteFrame.Value, part.Value));
                    break;
            }
        }

        return sb.ToString();
    }

    public bool TryParse(string filename, AssetType type, out AssetPath path)
    {
        var m = _regex.Match(filename);
        if (!m.Success)
        {
            path = default;
            return false;
        }

        var idGroup = m.Groups["Id"];
        var subAssetGroup = m.Groups["SubAsset"];
        var paletteGroup = m.Groups["Palette"];
        var pframeGroup = m.Groups["PFrame"];
        int id = idGroup.Success ? int.Parse(idGroup.Value) : -1;
        int subAsset = subAssetGroup.Success ? int.Parse(subAssetGroup.Value) : 0;
        int? paletteId = paletteGroup.Success ? (int?)int.Parse(paletteGroup.Value) : null;
        int? paletteFrame = pframeGroup.Success ? (int?)int.Parse(pframeGroup.Value) : null;

        path = new AssetPath(new AssetId(type, id), subAsset, paletteId, m.Groups["Name"].Value, paletteFrame);
        return true;
    }

    public string WilcardForId(AssetId id)
    {
        var sb = new StringBuilder();
        foreach (var part in _parts)
        {
            switch (part.Type)
            {
                case PartType.Id: sb.Append(FormatInt(id.Id, part.Value)); break;
                case PartType.Text: sb.Append(part.Value); break;
                case PartType.SubAsset: sb.Append('*'); break;
                case PartType.Name: sb.Append('*'); break;
                case PartType.IgnoreNum: sb.Append('*'); break;
                case PartType.Palette: sb.Append('*'); break;
                case PartType.PaletteFrame: sb.Append('*'); break;
            }
        }

        return sb.ToString();
    }
}
