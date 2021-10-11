using System.Linq;
using UAlbion.Formats.Scripting;

namespace UAlbion.Formats.Tests
{
    public static class TestGraphs
    {
        static DummyNode D(int num) => new(num.ToString());
        static DummyNode D(string name) => new(name);
        static DummyNode[] BuildNodes(int count) => Enumerable.Range(0, count).Select(D).ToArray();

        /* Sequence: 0 -> 1 -> 2  = 0;1;2 */
        public static readonly ControlFlowGraph Sequence = new(0, BuildNodes(3),
            new[] { (0, 1, true), (1, 2, true) });

        /* IfThen:
           0--\   if(0) { 1 }; 2
           t  |
           v  |
           1  f
           v  |
           2<-/ */
        public static readonly ControlFlowGraph IfThen = new(0, BuildNodes(3),
            new[] { (0, 1, true), (1, 2, true), (0, 2, false) });

        /* IfThenElse:
        0--\  if(0) { 1 } else { 2 }; 3
        t  f
        v  v
        1  2
        v  |
        3<-/ */
        public static readonly ControlFlowGraph IfThenElse = new(0, BuildNodes(4),
            new[] { (0, 1, true), (1, 3, true), (0, 2, false), (2, 3, true) });

        /* Simple while loop:
            0
            v
         /--1<-\   0; while(1) { }; 2
         f  t  |
         |  \--/
         |
         \->2 */
        public static readonly ControlFlowGraph SimpleWhileLoop = new(0, BuildNodes(3),
            new[] { (0, 1, true), (1, 1, true), (1, 2, false) });

        /* WhileLoop:
            0
            v
         /--1<-\   0; while(1) { 2 }; 3
         f  t  |
         |  v  |
         |  2--/
         |
         \->3 */
        public static readonly ControlFlowGraph WhileLoop = new(0, BuildNodes(4),
            new[] { (0, 1, true), (1, 2, true), (2, 1, true), (1, 3, false) });

        /* DoWhileLoop:
            0
            v
            1<-\   0; do { 1; } while(2); 3
            v  |
            2-t/
            f
            v
            3 */
        public static readonly ControlFlowGraph DoWhileLoop = new(0, BuildNodes(4),
            new[] { (0, 1, true), (1, 2, true), (2, 1, true), (2, 3, false) });

        /* Graph1
           0
           |<----\   0;
           v     |   do { 1 } while (2 && 3);
           1-->2 |   while (4) { }
              /t |   5;
           v-f v t
        /->4<f-3-/
        \t/f
           v
           5 */
        public static readonly ControlFlowGraph Graph1 = new(0, BuildNodes(6),
            new[] { (0, 1, true), (1, 2, true), (2, 3, true), (2, 4, false), (4, 4, true), (3, 1, true), (4, 5, false) });

        /* Graph2
          0
          v
          1--f-----\
          t        |
          v        |
          2 --> 3  |
          ^     |  |
           \    v  v
            5<t-6<t4
                f  f
                v  |
                7<-/ */
        public static readonly ControlFlowGraph Graph2 = new(0, BuildNodes(8), new[] {
                (0, 1, true), (1, 2, true), (2, 3, true), (3, 6, true), (6, 5, true), (5, 2, true),
                (6, 7, false), (4, 6, true), (4, 7, false), (1, 4, false) });

        /* 0
           v
           1----------\
           t          f
           v          v
      /-/->2-f--\     8 <-\--\
      | |  t    |     |   |  |
      | |  v    |     v  10  |
      | \t-3    |     9-t-/  |
      |    f    |     f      |
      |    v    |     v      |
      |    4--\ | /t--11     |
      |    f  t | |   f      |
      |    v  | | 12  13     |
      \----5  | | |   v      |
              v | |   14 -t--/
              6</ |   f
              |   v   |
              |   15<-/
              v   |
              7 <-/ */
        public static readonly ControlFlowGraph LoopBranch = new(0, BuildNodes(16), new[] {
            (0, 1, true), (1, 2, true), (1, 8, false), (2, 3, true), (2, 6, false), (3, 2, true),
            (3, 4, false), (4, 5, false), (4, 6, true), (5, 2, true), (6, 7, true), (8, 9, true),
            (9, 10, true), (10, 8, true), (9, 11, false), (11, 13, false), (11, 12, true),
            (12, 15, true), (13, 14, true), (14, 8, true), (14, 15, false), (15, 7, true) });
        public static string LoopBranchCode => @"0;
if (1) {
    while(2) {
        if (3) {
            continue;
        }
        if (4) {
            break;
        }
        5;
    }
    6;
}
else {
    do {
        8;
        if (9) {
            10;
            continue;
        }
        if (11) {
            12;
            break;
        }
        13;
    } while(14);
    9;
}
7";


        /* 0
           v
           1 <------------\
           v              t
           2 -f-> 8 > 9 > 10
           t              f
           v              |
           3              |
           v              |
       /-f-4-t-\          |
       v       v          |
       5-->7<--6          |
           |              |
           11 <-----------/*/
        public static readonly ControlFlowGraph BreakBranch = new(0, BuildNodes(12), new[] {
            (0, 1, true), (1, 2, true), (2, 3, true), (2, 8, false), (3, 4, true), (4, 5, false), (4, 6, true),
            (5, 7, true), (6, 7, true), (7, 11, true), (8, 9, true), (9, 10, true), (10, 1, true), (10, 11, false) });
        public static string BreakBranchCode => @"0;
do {
    1; 
    if (2) {
        3;
        if (4) {
            6;
        } else {
            5;
        }
        7;
        break;
    }
    8;
    9;
} while (10);
11;";

        /* 0
           v
           1 <----\
           v      t
           2 -f-> 5
           t      f
           v      |
           3      |
           v      |
           4 <---/*/
        public static readonly ControlFlowGraph BreakBranch2 = new(0, BuildNodes(6), new[] {
            (0, 1, true), (1, 2, true), (2, 3, true), (2, 5, false), (3, 4, true), (5, 1, true), (5, 4, false)
        });
        public static string BreakBranch2Code => @"0;
do {
    1; 
    if (2) {
        3;
        break;
    }
} while (5);
4;";

        /*########################\#/#########################\  if (A)
        #  No More Gotos figure 3  #                          #  {
        #                          #                          #     do {
        #       /f-A--t\           #       /f-0--t\           #         while (c1) { n1 }
        #      /        \          #      /        \          #         if (c2) {
        #     /          \         #     /          \         #             n2;
        #    /           |         #    /           |         #             break;
        #    b1     /--->c1 <-\    #     1     /---> 3 <-\    #         }
        #  f/  \t   |   f| \t |    #  f/  \t   |   f| \t |    #         n3
        #  n4   b2  |   c2  -n1    #  12    2  |    4  - 9    #     } while (c3);
        #  |   / |  |   / \        #  |   / |  |   / \        #  }
        #  | f/  t  |  f|   \t     #  | f/  t  |  f|   \t     #  else
        #  | /   |  |  n3    n2    #  | /   |  |  11    10    #  {
        #  n5    n6 |   |     |    #  13    14 |   |     |    #      if(!b1)
        #    \  /   |    \    |    #    \  /   |    \    |    #          n4
        #    n7     \     \   |    #    15     \     \   |    #      if (b1 && b2)
        #     \      \<-t-c3  |    #     \      \<--t 5  |    #          n6
        #      \           |  |    #      \           |  |    #      else
        #  /---> d1        f  |    #  /--->  6        f  |    #          n5
        #  |   t/  \f      |  |    #  |   t/  \f      |  |    #      n7
        #  |  d3 /t-d2     |  |    #  |   8 /t- 7     |  |    #      while ((d1 && d3) || (!d1 && d2)) {
        #  | /t X     \f   |  |    #  | /t X     \f   |  |    #          n8
        #  n8<-/ \     \   |  |    #  16<-/ \     \   |  |    #      }
        #         \f    | /   |    #         \f    | /   |    #  }
        #          \    |/   /     #          \    |/   /     #  n9
        #           ---n9----      #           ---17----      #
        \#########################/#\########################*/
        public static readonly ControlFlowGraph NoMoreGotos3 =
            new(0, new[] {
                D("A"),  // 0
                D("b1"), // 1
                D("b2"), // 2
                D("c1"), // 3
                D("c2"), // 4
                D("c3"), // 5
                D("d1"), // 6
                D("d2"), // 7
                D("d3"), // 8
                D("n1"), // 9
                D("n2"), // 10
                D("n3"), // 11
                D("n4"), // 12
                D("n5"), // 13
                D("n6"), // 14
                D("n7"), // 15
                D("n8"), // 16
                D("n9"), // 17
            }, new[] {
                ( 0, 1, false), // ( A,!b1),
                ( 0, 3, true),  // ( A,c1),
                ( 1, 2, true),  // (b1,b2),
                ( 1,12, false), // (b1,!n4),
                (12,13, true),  // (n4,n5),
                ( 2,13, false), // (b2,!n5),
                ( 2,14, true),  // (b2,n6),
                (14,15, true),  // (n6,n7),
                (13,15, true),  // (n5,n7),
                (15, 6, true),  // (n7,d1),
                ( 6, 8, true),  // (d1,d3),
                ( 6, 7, false), // (d1,!d2),
                ( 8,16, true),  // (d3,n8),
                ( 7,16, true),  // (d2,n8),
                (16, 6, true),  // (n8,d1),
                ( 8,17, false), // (d3,!n9),
                ( 7,17, false), // (d2,!n9),
                ( 3, 9, true),  // (c1,n1),
                ( 9, 3, true),  // (n1,c1),
                ( 3, 4, false), // (c1,!c2),
                ( 4,11, false), // (c2,!n3),
                ( 4,10, true),  // (c2,n2),
                (10,17, true),  // (n2,n9),
                (11, 5, true),  // (n3,c3),
                ( 5, 3, true),  // (c3,c1),
                ( 5,17, false), // (c3,!n9),
            });

        public static readonly ControlFlowGraph NoMoreGotos3Reversed =
            new(17, new[] {
                D("A"),  // 0
                D("b1"), // 1
                D("b2"), // 2
                D("c1"), // 3
                D("c2"), // 4
                D("c3"), // 5
                D("d1"), // 6
                D("d2"), // 7
                D("d3"), // 8
                D("n1"), // 9
                D("n2"), // 10
                D("n3"), // 11
                D("n4"), // 12
                D("n5"), // 13
                D("n6"), // 14
                D("n7"), // 15
                D("n8"), // 16
                D("n9"), // 17
            }, new[] {
                ( 1,  0, false), // (b1, !A),
                ( 3,  0, true),  // (c1,  A),
                ( 2,  1, true),  // (b2, b1),
                (12,  1, false), // (n4,!b1),
                (13, 12, true),  // (n5, n4),
                (13,  2, false), // (n5,!b2),
                (14,  2, true),  // (n6, b2),
                (15, 14, true),  // (n7, n6),
                (15, 13, true),  // (n7, n5),
                ( 6, 15, true),  // (d1, n7),
                ( 8,  6, true),  // (d3, d1),
                ( 7,  6, false), // (d2,!d1),
                (16,  8, true),  // (n8, d3),
                (16,  7, true),  // (n8, d2),
                ( 6, 16, true),  // (d1, n8),
                (17,  8, false), // (n9,!d3),
                (17,  7, false), // (n9,!d2),
                ( 9,  3, true),  // (n1, c1),
                ( 3,  9, true),  // (c1, n1),
                ( 4,  3, false), // (c2,!c1),
                (11,  4, false), // (n3,!c2),
                (10,  4, true),  // (n2, c2),
                (17, 10, true),  // (n9, n2),
                ( 5, 11, true),  // (c3, n3),
                ( 3,  5, true),  // (c1, c3),
                (17,  5, false), // (n9,!c3),
            });

        public static readonly ControlFlowGraph NoMoreGotos3Region1 =
            new(0, new[] {
                D("A"),  // 0
                D("c1"), // 1
                D("c2"), // 2
                D("c3"), // 3
                D("n1"), // 4
                D("n2"), // 5
                D("n3"), // 6
                D("n9"), // 7
            }, new[] {
                (0, 1, true),
                (1, 2, false),
                (1, 4, true),
                (4, 1, true),
                (2, 5, true),
                (2, 6, false),
                (3, 1, true),
                (3, 7, false),
                (5, 7, true),
                (6, 3, true)
            });

        public static string NoMoreGotos3Region1Code => 
@"A;
do {
    while (c1) {
        n1;
    }
    if (c2) {
        n2;
        break;
    }
    n3;
} while (c3);";

        public static readonly ControlFlowGraph NoMoreGotos3Region2 =
            new(0, new[]
            {
                D("A"),  // 0
                D("b1"), // 1
                D("b2"), // 2
                D("n4"), // 3
                D("n5"), // 4
                D("n6"), // 5
                D("n7"), // 6
            }, new[] {
                (0, 1, true),
                (1, 2, true),
                (1, 3, false),
                (2, 4, false),
                (2, 5, true),
                (3, 4, true),
                (4, 6, true),
                (5, 6, true)
            });

        public static string NoMoreGotos3Region2Code => 
@"A;
if (!b1) {
    n4;
}
if (b1 && b2) {
    n6;
}
else {
    n5;
}
n7;";

        public static readonly ControlFlowGraph NoMoreGotos3Region3 =
            new(0, new[]
            {
                D("start"), // 0
                D("d1"), // 1
                D("d2"), // 2
                D("d3"), // 3
                D("n8"), // 4
                D("n9"), // 5
            }, new[] {
                (0, 1, true),
                (1, 2, false),
                (1, 3, true),
                (2, 4, true),
                (2, 5, false),
                (3, 4, true),
                (3, 5, false),
                (4, 1, true),
            });
        public static string NoMoreGotos3Region3Code => 
@"start;
while ((d1 && d3) || (!d1 && d2)) {
    n8;
}
n9;";

        /* ZeroK if example
              0
             / \
            f   t
            v    \
            1     | if (!0) {
            v     |     1
            2     |     if (!2) { 3; 4 }
            f\    | }
            v t   | 5
            3  \  |
            v  |  |
            4  |  |
            |  v  |
            \->5<-/
        */

        /* ZeroK loop example 1
   0->1->2->3->4
               v
 /------------>5
 |             |
 |             v
 |        /----6<-----\
 |        |    f      |
 |        |    v      |
 |        t    7-t--\ |
 |        |    f    | |
 |        |    v    v |
 |        | /--8-f->9-/
 |        | t
 |        \ v
 |         10
 |        / \
 |       /   \
 |       f    \
 |       v     \
 |      11      t
 |       v      |
 | /-/->12      |
 | | |   v      |
 | | |  13----\ |
 | | t   f    t |
 | | |   v    | |
 | | \--14    | |
 | |     f    | |
 | |     v    | |
 | |    15--\ | |
 | t     f  t | |
 | |     v  | | |
 | \----16  | | |
 |       f  | | |
 |       v  | | |
 |   /--17<-/-/-/
 |   |   f
 t   t   v
 \---+--18
     |   f
     |   v
     \->19
        */

        /* ZeroK loop example 2
0-1-2-3-4
        v
        5
        v
     /->6--\
     |  f  t
     \--7  |
           v
       /-t-8
       |   f
       |   v
       |   9
       |   v
       |  10<-\
       |   v  t
       |  11--/
       |   v
       \->12 */

        /* ZeroK SESE example
           0
           v        0
           1--t--\  if(1) { 3; 4 }
           f     |  else {
           v     3
           2     |
           v     |
/----------6     |
|          f     |
|          v     |
|          7     |
|          v     |
|  /-------8     |
|  f       t     |
|  |       v     |
|  |      10     |
|  |       v     |
|  | /----11     |
|  | t     f     |
|  | |     v     |
|  | |    12     |
|  | |     v     |
|  | |    13     |
|  | |   /  \    |
|  | |  t    f   |
|  | |  v    v   |
|  \-\->9    14  |
|        \  /    |
|         Y      |
|         v      |
|         4<-----/
|         v
\-------->5 */
        public static readonly ControlFlowGraph ZeroKSese = new(0, BuildNodes(15), new[] {
            (0, 1, true),
            (1, 2, false),
            (1, 3, true),
            (2, 6, true),
            (3, 4, true),
            (4, 5, true),
            (6, 5, true),
            (6, 7, false),
            (7, 8, true),
            (8, 9, false),
            (8, 10, true),
            (9, 4, true),
            (10, 11, true),
            (11, 9, true),
            (11, 12, false),
            (12, 13, true),
            (13, 14, false),
            (13, 9, true)
        });
    }
}
