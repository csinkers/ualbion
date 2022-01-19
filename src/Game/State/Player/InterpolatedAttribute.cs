using System;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player;

public class InterpolatedAttribute : ICharacterAttribute
{
    readonly Func<ICharacterAttribute> _a;
    readonly Func<ICharacterAttribute> _b;
    readonly Func<float> _getLerp;

    public InterpolatedAttribute(Func<ICharacterAttribute> a, Func<ICharacterAttribute> b, Func<float> getLerp)
    {
        _a = a;
        _b = b;
        _getLerp = getLerp;
    }

    public ushort Current => (ushort)ApiUtil.Lerp(_a().Current, _b().Current, _getLerp());
    public ushort Max => (ushort)ApiUtil.Lerp(_a().Max, _b().Max, _getLerp());
    public ushort Boost => (ushort)ApiUtil.Lerp(_a().Boost, _b().Boost, _getLerp());
    public ushort Backup => (ushort)ApiUtil.Lerp(_a().Backup, _b().Backup, _getLerp());
}