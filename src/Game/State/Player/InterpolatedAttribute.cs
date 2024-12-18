﻿using System;
using UAlbion.Api;
using UAlbion.Formats.Assets.Sheets;

namespace UAlbion.Game.State.Player;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
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
    public override string ToString() => $"[{Current}/{Max}]{(Boost > 0 ? $"+{Boost}" : "")}{(Backup > 0 ? $" (was {Backup})" : "")}";
}
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix