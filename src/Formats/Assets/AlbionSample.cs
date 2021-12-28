namespace UAlbion.Formats.Assets;

public class AlbionSample : ISample
{
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int BytesPerSample { get; set; }
    public byte[] Samples { get; set; } // Setter for JSON
}