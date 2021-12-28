using System;

namespace UAlbion.Core.Visual;

public class SpriteSelectedEventArgs : EventArgs
{
    public Action<object> RegisterHit { get; }
    public bool Handled { get; set; }

    public SpriteSelectedEventArgs(Action<object> registerHit)
    {
        RegisterHit = registerHit;
    }
}