﻿using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.ScriptEvents;

[Event("party_member_text")] // USED IN SCRIPT
public class PartyMemberTextEvent : Event
{
    [EventPart("member_id")] public PartyMemberId MemberId { get; }
    [EventPart("text_id")] public byte TextId { get; }
    public PartyMemberTextEvent(PartyMemberId memberId, byte textId)
    {
        MemberId = memberId;
        TextId = textId;
    }
}