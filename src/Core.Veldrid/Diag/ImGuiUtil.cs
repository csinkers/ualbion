using System;
using System.Text;

namespace UAlbion.Core.Veldrid.Diag;

public static class ImGuiUtil
{
    public static string GetString(byte[] buffer)
    {
        var raw = Encoding.UTF8.GetString(buffer);
        int zeroIndex = raw.IndexOf('\0', StringComparison.Ordinal);
        return zeroIndex == -1 ? raw : raw[..zeroIndex];
    }
}