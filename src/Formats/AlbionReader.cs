using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats;

public class AlbionReader : ReaderSerdes
{
    readonly BinaryReader _br;

    public AlbionReader(BinaryReader br, long maxLength = 0, Action disposeAction = null)
        : base(br,
            maxLength == 0 ? br?.BaseStream.Length ?? 0 : maxLength,
            FormatUtil.BytesTo850String,
            ApiUtil.Assert,
            disposeAction)
    {
        _br = br;
    }

    protected override void Dispose(bool disposing)
    {
        _br.Dispose();
        base.Dispose(disposing);
    }
}