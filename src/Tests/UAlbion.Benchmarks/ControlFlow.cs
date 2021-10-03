using BenchmarkDotNet.Attributes;
using UAlbion.Formats.Tests;

namespace UAlbion.Benchmarks
{
    public class ControlFlow
    {
        [Benchmark]
        public void Cyclic1()
        {
            TestGraphs.Sequence.IsCyclic();
            TestGraphs.IfThen.IsCyclic();
            TestGraphs.IfThenElse.IsCyclic();
            TestGraphs.WhileLoop.IsCyclic();
            TestGraphs.DoWhileLoop.IsCyclic();
            TestGraphs.Graph1.IsCyclic();
            TestGraphs.Graph2.IsCyclic();
            TestGraphs.NoMoreGotos3.IsCyclic();
        }
    }
}