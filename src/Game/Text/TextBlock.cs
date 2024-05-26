using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public class TextBlock // Logical segment of text where all glyphs share the same formatting.
{
    public TextBlock() : this(BlockId.MainText, string.Empty) { }
    public TextBlock(BlockId blockId) : this(blockId, string.Empty) { }
    public TextBlock(string text) : this(BlockId.MainText, text) { }
    public TextBlock(BlockId blockId, string text)
    {
        BlockId = blockId;
        Text = text ?? "";
        InkId = Base.Ink.White;
    }

    string _text;

    public string Raw { get; set; }
    public BlockId BlockId { get; }
    public string Text { get => _text; set => _text = value ?? ""; }
    public InkId InkId { get; set; }
    public TextStyle Style { get; set; }
    public TextAlignment Alignment { get; set; }
    public TextArrangementFlags ArrangementFlags { get; set; }
    public IEnumerable<WordId> Words => _words ?? Enumerable.Empty<WordId>();
    public void AddWord(WordId word) { _words ??= new HashSet<WordId>(); _words.Add(word); }
    HashSet<WordId> _words;

    public override string ToString() => $"[\"{Text}\" {InkId} {Style} {Alignment} {ArrangementFlags}]";

    public bool IsMergeableWith(TextBlock other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return
            other.BlockId == BlockId &&
            (string.IsNullOrWhiteSpace(other.Text) ||
             other.InkId == InkId &&
             other.Style == Style &&
             other.Alignment == Alignment &&
             other.ArrangementFlags == ArrangementFlags);
    }

    public void Merge(TextBlock other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (string.IsNullOrEmpty(other.Text))
            Text += " ";
        else
            Text += other.Text;

        if (other._words != null)
        {
            _words ??= new HashSet<WordId>();
            _words.UnionWith(other._words);
        }
    }
}