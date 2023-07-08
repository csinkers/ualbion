﻿using System;
using System.Text;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers;

public class Utf8Loader : IAssetLoader<string>
{
    public string Serdes(string existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if(s.IsWriting())
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var bytes = Encoding.UTF8.GetBytes(existing);
            s.Bytes(null, bytes, bytes.Length);
            return existing;
        }
        else
        {
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((string) existing, s, context);
}
