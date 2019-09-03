using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Api
{
    public static class Util
    {
        public static long LCM(IEnumerable<long> numbers) => numbers.Aggregate(LCM);
        public static long LCM(long a, long b) => Math.Abs(a * b) / GCD(a, b);
        public static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    }
}
