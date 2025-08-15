using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save;

public class VisitedEvent
{
    public const int SizeOnDisk = 6;
    public byte Unk0 { get; private set; }
    public EventSetId EventSetId { get; private set; }
    public ActionType Type { get; private set; }
    public AssetId Argument { get; private set; }

    VisitedEvent() { }
    public VisitedEvent(EventSetId eventSetId, ActionType type, AssetId argument)
    {
        if (argument.Type != type.GetAssetType())
            ApiUtil.Assert($"Expected argument to have asset type {type.GetAssetType()} to correspond with action type ({type}, but it was {argument}");

        EventSetId = eventSetId;
        Type = type;
        Argument = argument;
    }

    public static VisitedEvent Serdes(SerdesName n, VisitedEvent u, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);

        u ??= new VisitedEvent();
        s.Begin();
        u.Unk0 = s.UInt8(nameof(Unk0), u.Unk0);
        u.EventSetId = EventSetId.SerdesU16(nameof(EventSetId), u.EventSetId, mapping, s);
        u.Type = s.EnumU8(nameof(Type), u.Type);
        u.Argument = AssetId.SerdesU16(nameof(Argument), u.Argument, u.Type.GetAssetType(), mapping, s);

        if (s.IsCommenting())
            s.Comment(u.ToString());

        s.End();
        return u;
    }

    public override string ToString() => $"{Unk0} {EventSetId} {Type} {Argument}";

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode() =>
        (int)HashUtil.Fnv1A()
            .Combine(Unk0)
            .Combine(EventSetId.ToUInt32())
            .Combine((int)Type)
            .Combine(Argument.ToUInt32())
            .Hash;
    // ReSharper restore NonReadonlyMemberInGetHashCode

    /*
     Logical to textual word id mapping clues:

        180 =>           (DDT)
        182 => 684 (502) (AI)
        183 => 685 (502) (Ned)
        189 => 691 (502) (over-c)
        190 => 692 (502) (complex)
        191 => 693 (502) (Snoopy)
        192 => 694 (502) (environmentalist)
        193 => 695 (502) (captain)
        194 => 697 (503) (Brandt)
        200 => 703 (503) (navigation officer)
        201 => 704 (503) (mathematician)
        202 => 705 (503) (flight)
     */
}
