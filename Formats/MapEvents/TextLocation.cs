namespace UAlbion.Formats.MapEvents
{
    public enum TextLocation : byte
    {
        TextInWindow = 0,
        TextInWindowWithPortrait = 2,
        Conversation = 4,
        QuickInfo = 6,
        ConversationQuery = 9, // Show text in main conv window, then show options dlg without standard options.
        TextInWindowWithPortrait2 = 10, // Not sure how this one differs
        // TextInWindowWithNpcPortrait,
        ConversationOptions = 11, // Show standard and BLOK conversation options.
        StandardOptions = 13,
        // DialogQuestion,
        // DialogResponse,
        // AddDefaultDialogOption,
        // ListDefaultDialogOptions
    }
}