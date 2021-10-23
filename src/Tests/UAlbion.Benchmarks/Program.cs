using System;
using BenchmarkDotNet.Running;

namespace UAlbion.Benchmarks
{
    static class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<Blitting>();
            Console.ReadLine();
        }
    }
}
