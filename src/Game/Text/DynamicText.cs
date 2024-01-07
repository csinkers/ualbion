using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public class DynamicText : IText
{
    public delegate IEnumerable<TextBlock> GeneratorFunc();
    readonly GeneratorFunc _generator;
    readonly Func<int, int> _getVersion;
    int _version = 1;
#if DEBUG
    string _lastText;
    public override string ToString()
    {
        if (_lastText != null)
            return _lastText;

        var sb = new StringBuilder();
        var blockId = BlockId.MainText;
        var words = new List<WordId>();

        void WriteWords()
        {
            if (words.Any())
            {
                sb.Append(" (");
                sb.Append(string.Join(", ", words.Select(x => x.ToString())));
                sb.Append(')');
                words.Clear();
            }
        }

        foreach (var block in _generator())
        {
            if (block.BlockId != blockId)
            {
                WriteWords();
                sb.AppendLine();
                sb.Append("Block");
                sb.Append(block.BlockId);
                sb.Append(": ");
                blockId = block.BlockId;
            }

            foreach (var word in block.Words)
                words.Add(word);
            sb.Append(block.Text);
        }

        WriteWords();
        _lastText = sb.ToString();
        return _lastText;
    }
#endif

    public DynamicText(GeneratorFunc generator)
    {
        _generator = generator;
        _getVersion = _ => _version;
    }

    public DynamicText(GeneratorFunc generator, Func<int, int> getVersion)
    {
        _generator = generator;
        _getVersion = getVersion;
    }

    public int Version => _getVersion(_version);
    public void Invalidate() => _version++;
    public IEnumerable<TextBlock> GetBlocks() => _generator();
}