﻿using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events.Inventory;

[Event("inv:give", "Give the currently held item(s) to another party member.")]
public class InventoryGiveItemEvent : GameEvent
{
    public InventoryGiveItemEvent(PartyMemberId memberId) { MemberId = memberId; }
    [EventPart("memberId", "The party member to give to.")] public PartyMemberId MemberId { get; }
}