//#if false // Commented out for now to avoid breaking github actions
using System;
using System.Linq;
using System.Text;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Scripting;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class DecompilerTests
    {
        static string Print(ICfgNode tree)
        {
            var sb = new StringBuilder();
            tree.ToPseudocode(sb, "");
            return sb.ToString().Replace(Environment.NewLine, "\n");
        }

        static string Print(ControlFlowGraph graph)
        {
            var sb = new StringBuilder();
            foreach(var node in graph.GetDfsOrder())
                graph.Nodes[node].ToPseudocode(sb, "");
            return sb.ToString().Replace(Environment.NewLine, "\n");
        }

        [Fact]
        public void ReduceSequenceTest()
        {
            var result = 
                Decompiler.ReduceSequences(
                    Decompiler.ReduceSequences(TestGraphs.Sequence));
            var pseudo = Print(result);
            Assert.Equal("0\n1\n2\n", pseudo);
        }

        [Fact]
        public void ReduceIfThenTest()
        {
            var result = Decompiler.ReduceIfThen(TestGraphs.IfThen);
            var pseudo = Print(result);
            Assert.Equal("if (0\n) {\n    1\n}\n2\n", pseudo);
        }

        [Fact]
        public void ReduceIfThenElseTest()
        {
            var result = Decompiler.ReduceIfThenElse(TestGraphs.IfThenElse);
            var pseudo = Print(result);
            Assert.Equal("if (0\n) {\n    1\n} else {\n    2\n}\n3\n", pseudo);
        }

        [Fact]
        public void ReduceSimpleWhileTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.SimpleWhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\nwhile (1\n) {\n}\n2\n", pseudo);
        }

        [Fact]
        public void ReduceWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.WhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\nwhile (1\n) {\n    2\n}\n3\n", pseudo);
        }

        [Fact]
        public void ReduceDoWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.DoWhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\ndo {    2\n} while (1\n)\n3\n", pseudo);
        }

        [Fact]
        public void SequenceTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.Sequence);
            var pseudo = Print(result);
            Assert.Equal("0\n1\n2\n", pseudo);
        }

        [Fact]
        public void IfThenTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.IfThen);
            var pseudo = Print(result);
            Assert.Equal("if (0\n) {\n    1\n}\n2\n", pseudo);
        }

        [Fact]
        public void IfThenElseTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.IfThenElse);
            var pseudo = Print(result);
            Assert.Equal("if (0\n) {\n    1\n} else {\n    2\n}\n3\n", pseudo);
        }

        [Fact]
        public void SimpleWhileTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.SimpleWhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\nwhile (1\n) {\n}\n2\n", pseudo);
        }

        [Fact]
        public void WhileTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.WhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\nwhile (1\n) {\n    2\n}\n3\n", pseudo);
        }

        [Fact]
        public void DoWhileTest()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.DoWhileLoop);
            var pseudo = Print(result);
            Assert.Equal("0\ndo {    2\n} while (1\n)\n3\n", pseudo);
        }
#if false // Disabled until SESE structuring is ready
        [Fact]
        public void NoMoreGotos3Test()
        {
            var result = Decompiler.SimplifyGraph(TestGraphs.NoMoreGotos3);
            var pseudo = Print(result);
            Assert.Equal(
                @"something",
                pseudo);
        }

        [Fact]
        public void DecompileTest()
        {
            const string script = 
@"!0?1:2: query_verb IsTrue 0 Examine
 1=>!: map_text MapText.Jirinaar 37 NoPortrait None // ""The door to the house of the Hunter Clan. It is secured with a lock.""
!2?3:!: inv:door Door.HunterClanFrontDoor MapText.Jirinaar Item.HunterClanKey 100 32 33
!3?4:!: query_previous_action_result IsTrue 0 0
 4=>5: modify_unk2 0 0 0 0 101 0
 5=>!: teleport Map.HunterClan 69 67 Unchanged 255 0";
            var events = EventNode.ParseScript(script);
            var result = Decompiler.Decompile(events.Cast<IEventNode>().ToList());
            var pseudo = Print(result);
            Assert.Equal(
                @"something",
                pseudo);
        }
#endif
    }
}
//#endif