namespace UAlbion.Formats.Assets;

public interface ISample
{
    int SampleRate { get; set; }
    int Channels { get; set; }
    int BytesPerSample { get; set; }
    byte[] Samples { get; set; }
}