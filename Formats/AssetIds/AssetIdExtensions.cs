using System;

namespace UAlbion.Formats.AssetIds
{
    public static class AssetIdExtensions
    {
        public static SmallPortraitId ToPortraitId(this PartyCharacterId partyCharacterId) =>
            partyCharacterId switch
            {
                PartyCharacterId.Tom      => SmallPortraitId.Tom,
                PartyCharacterId.Rainer   => SmallPortraitId.Rainer,
                PartyCharacterId.Drirr    => SmallPortraitId.Drirr,
                PartyCharacterId.Sira     => SmallPortraitId.Sira,
                PartyCharacterId.Mellthas => SmallPortraitId.Mellthas,
                PartyCharacterId.Harriet  => SmallPortraitId.Harriet,
                PartyCharacterId.Joe      => SmallPortraitId.Joe,
                PartyCharacterId.Unknown7 => SmallPortraitId.Unknown7,
                PartyCharacterId.Khunag   => SmallPortraitId.Khunag,
                PartyCharacterId.Siobhan  => SmallPortraitId.Siobhan,
                _ => throw new ArgumentOutOfRangeException(nameof(partyCharacterId), partyCharacterId, null)
            };
    }
}
