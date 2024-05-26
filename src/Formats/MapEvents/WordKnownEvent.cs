using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("word_known")]
public class WordKnownEvent : ModifyEvent
{
    public static WordKnownEvent Serdes(WordKnownEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new WordKnownEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("byte3", 0);
        zeroes += s.UInt8("byte4", 0);
        zeroes += s.UInt8("byte5", 0);
        e.Word = WordId.SerdesU16(nameof(Word), e.Word, mapping, s);
        zeroes += s.UInt16("word8", 0);
        ApiUtil.Assert(zeroes == 0, "Expected fields 3,4,5,8 to be 0 in WordKnownEvent");
        return e;
    }

    WordKnownEvent() { }
    public WordKnownEvent(SwitchOperation operation, WordId door)
    {
        Operation = operation;
        Word = door;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("word")] public WordId Word { get; private set; }
    public override ModifyType SubType => ModifyType.WordKnown;
}