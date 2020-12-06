namespace UAlbion.Formats.MapEvents
{
    public enum TextLocation : byte
    {
        NoPortrait = 0,
        LeaderPortraitLeft = 1,
        PortraitLeft = 2,
        PortraitLeft2 = 3,
        Conversation = 4,
        Unk5 = 5,
        QuickInfo = 6,
        Unk7 = 7,
        ConversationQuery = 9, // Show text in main conv window, then show options dlg without standard options.
        PortraitLeft3 = 10, // Not sure how this one differs
        // TextInWindowWithNpcPortrait,
        ConversationOptions = 11, // Show standard and BLOK conversation options.
        StandardOptions = 13,
        // DialogQuestion,
        // DialogResponse,
        // AddDefaultDialogOption,
        // ListDefaultDialogOptions
    }
}
