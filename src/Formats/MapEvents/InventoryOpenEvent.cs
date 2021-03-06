﻿using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("inv:open", "Opens the given character's inventory")]
    public class InventoryOpenEvent : Event
    {
        public InventoryOpenEvent(PartyMemberId member) => PartyMemberId = member;
        [EventPart("member")] public PartyMemberId PartyMemberId { get; }
    }
}
