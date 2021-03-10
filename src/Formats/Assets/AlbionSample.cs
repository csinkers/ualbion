namespace UAlbion.Formats.Assets
{
    public class AlbionSample : ISample
    {
        public int SampleRate => 11025;
        public int Channels => 1;
        public int BytesPerSample => 1;
        public byte[] Samples { get; set; } // Setter for JSON
    }
}
