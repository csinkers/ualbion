using System;
using UAlbion.Formats.Ids;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Game.Gui.Dialogs;

public abstract class ConversationTrigger : IEquatable<ConversationTrigger>
{
    // Wish C# had proper discriminated unions like F#...
    public class Initial : ConversationTrigger { }
    public class Word : ConversationTrigger { public Word(WordId wordId) => WordId = wordId; public WordId WordId { get; } }
    public class Block : ConversationTrigger { public Block(int textId, int blockId) { TextId = textId; BlockId = blockId; }  public int TextId { get; } public int BlockId { get; } }

    public bool Equals(ConversationTrigger other) =>
        (this, other) switch
        {
            (Block blockA, Block blockB) => blockA.BlockId == blockB.BlockId && blockA.TextId == blockB.TextId,
            (Word wordA, Word wordB) => wordA.WordId == wordB.WordId,
            (Initial, Initial) => true,
            _ => false
        };

    public override bool Equals(object obj) =>
        !ReferenceEquals(null, obj) && 
        (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((ConversationTrigger)obj));

    public override int GetHashCode() =>
        this switch
        {
            Initial => 0,
            Block block => HashCode.Combine(1, block.TextId, block.BlockId),
            Word word => HashCode.Combine(2, word.WordId),
            _ => 0
        };
}
#pragma warning restore CA1034 // Nested types should not be visible
