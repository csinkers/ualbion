using System;
using System.Linq.Expressions;

namespace UAlbion.Config;

static class CastHelper<TFrom, TTo>
{
    static readonly Func<TFrom, TTo> Func = Build();
    static Func<TFrom, TTo> Build()
    {
        var p = Expression.Parameter(typeof(TFrom));
        return (Func<TFrom, TTo>)Expression.Lambda(Expression.Convert(p, typeof(TTo)), p).Compile();
    }

    public static TTo Cast(TFrom from) => Func(from);
}