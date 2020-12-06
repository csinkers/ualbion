module UAlbion.Util
open System.Diagnostics
open System.IO
open System.Reflection
open System.Text

let findBasePath () =
    let exeLocation = Assembly.GetExecutingAssembly().Location
    let mutable curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation))
    while (curDir <> null && not <| File.Exists(Path.Combine(curDir.FullName, "data", "config.json"))) do
        curDir <- curDir.Parent

    if curDir = null then failwith "Could not find base directory"
    curDir.FullName

let stringNotEmpty = not << System.String.IsNullOrEmpty
let runExe path arguments onComplete =
    async {
        let si = new ProcessStartInfo()
        si.CreateNoWindow <- true
        si.RedirectStandardOutput <- true
        si.RedirectStandardError <- true
        si.FileName <- path
        si.Arguments <- arguments
        si.UseShellExecute <- false

        use p = new Process()
        p.StartInfo <- si
        p.Start() |> ignore
        let result = p.StandardOutput.ReadToEnd()
        let errors = p.StandardError.ReadToEnd()
        p.WaitForExit()
        onComplete (result, errors)
    } |> Async.Start

let writeWavFile path (buffer:byte array) (sampleRate:uint32) (numChannels:uint16) (bytesPerSample:uint16) =
    use stream = File.OpenWrite(path)
    use bw = new BinaryWriter(stream)

    bw.Write(Encoding.ASCII.GetBytes("RIFF")) // Container format chunk

    let riffSizeOffset = stream.Position
    bw.Write(0) // Dummy write to start with, will be overwritten at the end.

    bw.Write(Encoding.ASCII.GetBytes("WAVE"))
    bw.Write(Encoding.ASCII.GetBytes("fmt ")) // Subchunk1 (format metadata)
    bw.Write(16)
    bw.Write(1us) // Format = Linear Quantisation
    bw.Write(1us) // NumChannels
    bw.Write(sampleRate) // SampleRate
    bw.Write(sampleRate * (uint32 numChannels) * (uint32 bytesPerSample)) // ByteRate
    bw.Write(numChannels * bytesPerSample) // BlockAlign
    bw.Write(bytesPerSample * 8us) // BitsPerSample

    bw.Write(Encoding.ASCII.GetBytes("data")) // Subchunk2 (raw sample data)
    bw.Write(buffer.Length)
    bw.Write(buffer)

    let totalLength = stream.Position // Write actual length to container format chunk
    stream.Position <- riffSizeOffset
    bw.Write((uint32 totalLength) - 8u)

    (* WAV file format:
0000000: 5249 4646 2e45 0100 5741 5645 666d 7420  RIFF.E..WAVEfmt
00: "RIFF"
04: int Size
    08: "WAVE"
        0c: "fmt "
        10: int Size = 0x10 = 16 (next chunk begins at 0c + 8 + 10 = 24)
        14: uint AudioFormat = 1 (PCM Linear quantization)
        16: uint NumChannels = 2
        18: int SampleRate = 0xac44 (44,100)
        1c: int ByteRate = 0x2b110 (176,400) (i.e. 2 bytes per sample)
0000010: 1000 0000 0100 0200 44ac 0000 10b1 0200  ........D.......
        20: uint BlockAlign = 4 (Channels * BytesPerSample)
        20: uint BitsPerSample = 0x10 (2 bytes per sample)

        24: "data"
        28: int Size = 0x1447c (83,068) (next chunk begins at 0x1447c + 24 + 8 = 144a8)
        2c: samples
0000020: 0400 1000 6461 7461 7c44 0100 0000 0000  ....data|D......

...

144a8: "LIST:\0"
144ae: uint16 0
144b0: "INFOIGNR"
144b8: int ?? = 6
144bc: "Blues"Z; "ICRD"Z; 0us; "2016-03-17"Z; 1uy;
       "IENG"Z; 0us; "Mike Koenig"Z; "CDifdD"Z; 0us; 68; 1; 0...



00144a0: 0000 0000 0000 0000 4c49 5354 3a00 0000  ........LIST:...
00144b0: 494e 464f 4947 4e52 0600 0000 426c 7565  INFOIGNR....Blue
00144c0: 7300 4943 5244 0b00 0000 3230 3136 2d30  s.ICRD....2016-0
00144d0: 332d 3137 0001 4945 4e47 0c00 0000 4d69  3-17..IENG....Mi
00144e0: 6b65 204b 6f65 6e69 6700 4344 6966 4400  ke Koenig.CDifD.
00144f0: 0000 4400 0000 0100 0000 0000 0000 0000  ..D.............
0014500: 0000 0000 0000 0000 0000 0000 0000 0000  ................
0014510: 0000 0000 0000 0000 0000 0000 0000 0000  ................
0014520: 0000 0000 0000 0000 0000 0000 0000 0000  ................
0014530: 0000 0000 0000                           ......

    *)
