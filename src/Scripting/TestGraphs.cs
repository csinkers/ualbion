using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public static class TestGraphs
{
    static Statement D(string name) => Emit.Statement(Emit.Name(name));
    /* Sequence: 1 -> 2 -> 3  = 1,2,3 */
    public static ControlFlowGraph Sequence => ControlFlowGraph.FromString("[0, 4, 5, 0+1 1+2 2+3 3+4]");

    /* IfThen:
       0
       v
       1--\   if(1) { 2 }, 3
       t  |
       v  |
       2  f
       v  |
       3<-/ */
    public static ControlFlowGraph IfThen => ControlFlowGraph.FromString("[0, 3, 4, 0+1 1+2 2+3 1-3]");

    /* IfThenElse:
    0
    v
    1--\  if(1) { 2 } else { 3 }, 4
    t  f
    v  v
    2  3
    v  |
    4<-/ */
    public static ControlFlowGraph IfThenElse => ControlFlowGraph.FromString("[0, 4, 5, 0+1 1+2 2+4 1-3 3+4]");

    /* Simple while loop:
        0
        v
     /--1<-\   0, while(1) { }, 2
     f  t  |
     |  \--/
     |
     \->2 */
    public static ControlFlowGraph SimpleWhileLoop => ControlFlowGraph.FromString("[0,2,3,0+1 1+1 1-2]");
    public static ControlFlowGraph NegativeSimpleWhileLoop => ControlFlowGraph.FromString("[0,2,3,0+1 1-1 1+2]");
    /* WhileLoop:
        0
        v
     /--1<-\   while(1) { 2 }
     f  t  |
     |  v  |
     |  2--/
     |
     \->3 */
    public static ControlFlowGraph WhileLoop => ControlFlowGraph.FromString("[0,3,4,0+1 1+2 2+1 1-3]");
    public static ControlFlowGraph NegativeWhileLoop => ControlFlowGraph.FromString("[0,3,4,0+1 1-2 2+1 1+3]");

    /* DoWhileLoop:
        0
        v
        1<-\   do { 1 } while(2)
        v  |
        2-t/
        f
        v
        3 */
    public static ControlFlowGraph DoWhileLoop => ControlFlowGraph.FromString("[0,3,4,0+1 1+2 2+1 2-3]");
    public static ControlFlowGraph NegativeDoWhileLoop => ControlFlowGraph.FromString("[0,3,4,0+1 1+2 2-1 2+3]");

    /* 0 > 1 <-\
           \---/  */
    public static ControlFlowGraph InfiniteLoop1 => ControlFlowGraph.FromString("[0,2,3,0+1 1+1]");

    /* 0 > 1 <-> 2 */
    public static ControlFlowGraph InfiniteLoop2 => ControlFlowGraph.FromString("[0,3,4,0+1 1+2 2+1]");

    /* Graph1
       0
       |<----\
       v     |   do { 1 } while (2 && 3)
       1-->2 |   while (4) { }
          /t |
       v-f v t
    /->4<f-3-/
    \t/f
       v
       5 */
    public static ControlFlowGraph Graph1 => ControlFlowGraph.FromString("[0,5,6,0+1 1+2 2+3 2-4 4+4 3+1 4-5]");
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
    public static ControlFlowGraph Graph2 => ControlFlowGraph.FromString("[0,7,8,0+1 1+2 2+3 3+6 6+5 5+2 6-7 4+6 4-7 1-4]");
    /* 0
       v
       1----------\
       t          f
       v          v
  /-/->2-f--\     7 <-\--\
  | |  t    |     |   |  |
  | |  v    |     v   9  |
  | \t-3    |     8-t-/  |
  |    f    |     f      |
  |    v    |     v      |
  |    4--\ | /t--10     |
  |    f  t | |   f      |
  |    v  | | 11  12     |
  \----5  | | |   v      |
          v | |   13 -t--/
          6</ |   f
          |   v   |
          |   14<-/
          v   |
          15 <-/ */
    public static ControlFlowGraph LoopBranch => ControlFlowGraph.FromString("[0,15,16,0+1 1+2 2+3 3-4 4-5 2-6 3+2 4+6 5+2 6+15 1-7 7+8 8+9 9+7 8-10 10+11 10-12 12+13 13+7 13-14 11+14 14+15]");
    public static string LoopBranchCode => @"if (1) {
    while (2) {
        if (!(3)) {
            if (4) {
                break
            }
            5
        }
    }
    6
} else {
    do {
        7
        if (8) {
            9
            continue
        } else {
            if (10) {
                11
                break
            }
            12
        }
    } while (13)
    14
}";

    public static ControlFlowGraph LoopBranchReduced => ControlFlowGraph.FromString("[0,9,10,0+1 1+2 1-3 2+9 3+4 4+5 4-6 5+3 6+7 7+3 7-8 8+9]");
    public static string LoopBranchReducedCode => @"if (1) {
    2
} else {
    do {
        3
        if (4) {
            5
            continue
        } else {
            6
        }
    } while (7)
    8
}";

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
    public static ControlFlowGraph BreakBranch => ControlFlowGraph.FromString("[0,11,12,0+1 1+2 2+3 2-8 3+4 4-5 4+6 5+7 6+7 7+11 8+9 9+10 10+1 10-11]");
    public static string BreakBranchCode => @"do {
    1
    if (2) {
        3
        if (4) {
            6
        } else {
            5
        }
        7
        break
    }
    8
    9
} while (10)";

    /* 0
       v
       1 <----\
       v      t
       2 -f-> 4
       t      f
       v      |
       3      |
       v      |
       5 <---/*/
    public static ControlFlowGraph BreakBranch2 => ControlFlowGraph.FromString("[0,5,6,0+1 1+2 2+3 2-4 3+5 4+1 4-5]");
    public static string BreakBranch2Code => @"do {
    1
    if (2) {
        3
        break
    }
} while (4)";

    /*########################\#/#########################\  if (A)
    #  No More Gotos figure 3  #                          #  {
    #                          #                          #     do {
    #       /f-A--t\           #        /f-0--t\          #         while (c1) { n1 }
    #      /        \          #       /        \         #         if (c2) {
    #     /          \         #      /          \        #             n2;
    #    /           |         #     /           |        #             break;
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
    public static ControlFlowGraph NoMoreGotos3 =>
        new(0, 17, [
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
            D("n9") // 17
        ], [
            ( 0, 1, CfgEdge.False), // ( A,!b1),
            ( 0, 3, CfgEdge.True),  // ( A,c1),
            ( 1, 2, CfgEdge.True),  // (b1,b2),
            ( 1,12, CfgEdge.False), // (b1,!n4),
            (12,13, CfgEdge.True),  // (n4,n5),
            ( 2,13, CfgEdge.False), // (b2,!n5),
            ( 2,14, CfgEdge.True),  // (b2,n6),
            (14,15, CfgEdge.True),  // (n6,n7),
            (13,15, CfgEdge.True),  // (n5,n7),
            (15, 6, CfgEdge.True),  // (n7,d1),
            ( 6, 8, CfgEdge.True),  // (d1,d3),
            ( 6, 7, CfgEdge.False), // (d1,!d2),
            ( 8,16, CfgEdge.True),  // (d3,n8),
            ( 7,16, CfgEdge.True),  // (d2,n8),
            (16, 6, CfgEdge.True),  // (n8,d1),
            ( 8,17, CfgEdge.False), // (d3,!n9),
            ( 7,17, CfgEdge.False), // (d2,!n9),
            ( 3, 9, CfgEdge.True),  // (c1,n1),
            ( 9, 3, CfgEdge.True),  // (n1,c1),
            ( 3, 4, CfgEdge.False), // (c1,!c2),
            ( 4,11, CfgEdge.False), // (c2,!n3),
            ( 4,10, CfgEdge.True),  // (c2,n2),
            (10,17, CfgEdge.True),  // (n2,n9),
            (11, 5, CfgEdge.True),  // (n3,c3),
            ( 5, 3, CfgEdge.True),  // (c3,c1),
            ( 5,17, CfgEdge.False) // (c3,!n9),
        ]);
    public const string NoMoreGotos3Code = @"if (A) {
    do {
        if (c1) {
            n1
            continue
        } else {
            if (c2) {
                n2
                break
            }
            n3
        }
    } while (c3)
} else {
    if (b1) {
        if (b2) {
            n6
        } else {
            goto L1
        }
    } else {
        n4
        L1:
        n5
    }
    n7
    loop {
        if (d1) {
            if (!(d3)) {
                break
            }
        } else {
            if (!(d2)) {
                break
            }
        }
        n8
    }
}
n9";

    public static ControlFlowGraph NoMoreGotos3Reversed =>
        new(17, 0, [
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
            D("n9") // 17
        ], [
            ( 1,  0, CfgEdge.False), // (b1, !A),
            ( 3,  0, CfgEdge.True),  // (c1,  A),
            ( 2,  1, CfgEdge.True),  // (b2, b1),
            (12,  1, CfgEdge.False), // (n4,!b1),
            (13, 12, CfgEdge.True),  // (n5, n4),
            (13,  2, CfgEdge.False), // (n5,!b2),
            (14,  2, CfgEdge.True),  // (n6, b2),
            (15, 14, CfgEdge.True),  // (n7, n6),
            (15, 13, CfgEdge.True),  // (n7, n5),
            ( 6, 15, CfgEdge.True),  // (d1, n7),
            ( 8,  6, CfgEdge.True),  // (d3, d1),
            ( 7,  6, CfgEdge.False), // (d2,!d1),
            (16,  8, CfgEdge.True),  // (n8, d3),
            (16,  7, CfgEdge.True),  // (n8, d2),
            ( 6, 16, CfgEdge.True),  // (d1, n8),
            (17,  8, CfgEdge.False), // (n9,!d3),
            (17,  7, CfgEdge.False), // (n9,!d2),
            ( 9,  3, CfgEdge.True),  // (n1, c1),
            ( 3,  9, CfgEdge.True),  // (c1, n1),
            ( 4,  3, CfgEdge.False), // (c2,!c1),
            (11,  4, CfgEdge.False), // (n3,!c2),
            (10,  4, CfgEdge.True),  // (n2, c2),
            (17, 10, CfgEdge.True),  // (n9, n2),
            ( 5, 11, CfgEdge.True),  // (c3, n3),
            ( 3,  5, CfgEdge.True),  // (c1, c3),
            (17,  5, CfgEdge.False) // (n9,!c3),
        ]);

    public static ControlFlowGraph NoMoreGotos3Region1 =>
        new(0, 7, [
            D("A"),  // 0
            D("c1"), // 1
            D("c2"), // 2
            D("c3"), // 3
            D("n1"), // 4
            D("n2"), // 5
            D("n3"), // 6
            D("n9") // 7
        ], [
            (0, 1, CfgEdge.True),
            (1, 2, CfgEdge.False),
            (1, 4, CfgEdge.True),
            (4, 1, CfgEdge.True),
            (2, 5, CfgEdge.True),
            (2, 6, CfgEdge.False),
            (3, 1, CfgEdge.True),
            (3, 7, CfgEdge.False),
            (5, 7, CfgEdge.True),
            (6, 3, CfgEdge.True)
        ]);

    public static string NoMoreGotos3Region1Code =>
        @"A
loop {
    if (c1) {
        n1
    } else {
        if (c2) {
            n2
            break
        }
        n3
        if (!(c3)) {
            break
        }
    }
}
n9";

    public static ControlFlowGraph NoMoreGotos3Region2 =>
        new(0, 6, [
            D("A"),  // 0
            D("b1"), // 1
            D("b2"), // 2
            D("n4"), // 3
            D("n5"), // 4
            D("n6"), // 5
            D("n7") // 6
        ], [
            (0, 1, CfgEdge.True),
            (1, 2, CfgEdge.True),
            (1, 3, CfgEdge.False),
            (2, 4, CfgEdge.False),
            (2, 5, CfgEdge.True),
            (3, 4, CfgEdge.True),
            (4, 6, CfgEdge.True),
            (5, 6, CfgEdge.True)
        ]);

    public const string NoMoreGotos3Region2Code = @"A, if (b1) { if (b2) { n6 } else { goto L1 } } else { n4, L1:, n5 }, n7";

    public static ControlFlowGraph NoMoreGotos3Region3 =>
        new(0, 5, [
            D("start"), // 0
            D("d1"), // 1
            D("d2"), // 2
            D("d3"), // 3
            D("n8"), // 4
            D("n9") // 5
        ], [
            (0, 1, CfgEdge.True),
            (1, 2, CfgEdge.False),
            (1, 3, CfgEdge.True),
            (2, 4, CfgEdge.True),
            (2, 5, CfgEdge.False),
            (3, 4, CfgEdge.True),
            (3, 5, CfgEdge.False),
            (4, 1, CfgEdge.True)
        ]);
    public static string NoMoreGotos3Region3Code => @"start, loop { if (d1) { if (!(d3)) { break } } else { if (!(d2)) { break } }, n8 }, n9";

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
|  \-\->9    5   |
|        \  /    |
|         Y      |
|         v      |
|         4<-----/
|         v
\-------->14 */
    public static ControlFlowGraph ZeroKSese => ControlFlowGraph.FromString("[0,14,15,0+1 1-2 1+3 2+6 3+4 4+14 6+14 6-7 7+8 8-9 8+10 9+4 10+11 11+9 11-12 12+13 13-5 13+9 5+4]");

    /* 0 ______
       v/      \
   /-t-1-f-\   |
   v   ^   v   |
   2---/   3-t-/
           f
           v
           4 */
    public const string NestedLoopCode = @"loop {
    if (1) {
        2
    } else {
        if (!(3)) {
            break
        }
    }
}";
    public static ControlFlowGraph NestedLoop => ControlFlowGraph.FromString("[0,4,5,0+1 1+2 2+1 1-3 3+1 3-4]");

    /* 0
       v
       1
      / \
     t   f
    /     \
   2       3
 /  \     / \
t    f   f   t
/      \ /     \
4        5       6
\       |      /
\      v     /
 ----> 7 <--- */
    public const string DiamondSeseCode =
        @"if (1) {
    if (2) {
        4
    } else {
        goto L1
    }
} else {
    if (3) {
        6
    } else {
        L1:
        5
    }
}";
    public static ControlFlowGraph DiamondSese => ControlFlowGraph.FromString("[0,7,8,0+1 1+2 1-3 2+4 2-5 3-5 3+6 4+7 5+7 6+7]");

    /*
    1
    v
    2--f-\
    t    |
    3--t\|
    f    |
    4--f\|
    t    v
    5    6
    |    |
    7 <--/ */
    public const string SeseExample1Code = "1, if (2) { if (3) { goto L1 } else { if (4) { 5 } else { L1:, 6 } } } else { goto L1 }, 7";
    public static ControlFlowGraph SeseExample1 => ControlFlowGraph.FromString("[0,8,9,0+1 1+2 2+3 3-4 4+5 5+7 2-6 3+6 4-6 6+7 7+8]");

    /*  0
        v
        1--f-> 3--f-------\
        v      |          |
        2      t          |
        |      v          |
        |      4          |
        |      |          |
        |  /-->5 --t->6--\|
        |  |   f          |
        |  |   7 --t->8--\|
        |  |   f          |
        |  |   9 --t->10-\|
        |  |   f          |
        |  \-f-11 -t->12-\|
        |                 |
        \------>13 <------/ */
    public const string MultiBreakCode = @"if (1) {
    2
} else {
    if (3) {
        4
        loop {
            if (5) {
                6
                break
            }
            if (7) {
                8
                break
            }
            if (9) {
                10
                break
            }
            if (11) {
                12
                break
            }
        }
    }
    13
}";
    public static ControlFlowGraph MultiBreak => ControlFlowGraph.FromString("[0,14,15,0+1 1+2 1-3 3+4 4+5 5+6 7+8 9+10 11+12 5-7 7-9 9-11 11-5 2+14 3-13 6+13 8+13 10+13 12+13 13+14]");
/* 0
   |
/--1---\
t  ^   l
|  f   |
3  v   |
|  2   |
|      |
\->4 <-/
   v
   5
 */
    public const string MultiBreak2Code = "loop { if (1) { 3, break }, 2 }, 4";
    public static ControlFlowGraph MultiBreak2 => ControlFlowGraph.FromString("[0,5,6,0+1 1+3 3+4 1l4 1-2 2-1 4+5]");



    /* 0
       v
2 <-f- 1
|      t      
| /--> 3
| |    |
| 5<-f-4
|      t
|      6
|      |
\----> 7 */
    public const string MidBreakLoopCode = "if (1) { loop { 3, if (4) { break }, 5 }, 6 } else { 2 }";
    public static ControlFlowGraph MidBreakLoop => ControlFlowGraph.FromString("[0,7,8,0+1 1-2 2+7 1+3 3+4 4+6 6+7 4-5 5+3]");

    public const string MultiBreakMap166Code = @"if (1) {
    2
} else {
    if (3) {
        4
        loop {
            if (5) {
                6
                break
            }
            if (7) {
                8
                break
            }
            if (9) {
                10
                break
            }
            if (11) {
                12
                break
            }
            if (13) {
                14
                break
            }
            if (15) {
                16
                break
            }
            if (17) {
                18
                break
            }
            if (19) {
                20
                break
            }
            if (21) {
                22
                break
            }
            if (23) {
                24
                break
            }
        }
    }
}";
    public static ControlFlowGraph MultiBreakMap166 => ControlFlowGraph.FromString(
        "[0,25,26,0+1 1+2 1-3 2+25 3+4 3-25 4+5 5+6 5-7 6+25 7+8 7-9 8+25 9+10 9-11 " +
        "10+25 11+12 11-13 12+25 13+14 13-15 14+25 15+16 15-17 16+25 17+18 17-19 " +
        "18+25 19+20 19-21 20+25 21+22 21-23 22+25 23+24 23-5 24+25]");

    public const string MultiBreakMap200Code = @"if (1) {
    2
} else {
    if (3) {
        if (4) {
            26
        } else {
            5
            loop {
                if (6) {
                    7
                    break
                }
                if (8) {
                    9
                    break
                }
                if (10) {
                    11
                    break
                }
                if (12) {
                    13
                    break
                }
                if (14) {
                    15
                    break
                }
                if (16) {
                    17
                    break
                }
                if (18) {
                    19
                    break
                }
                if (20) {
                    21
                    break
                }
                if (22) {
                    23
                    break
                }
                if (24) {
                    25
                    break
                }
            }
        }
    }
}";

    public static ControlFlowGraph MultiBreakMap200 => ControlFlowGraph.FromString(
        "[0,27,28,0+1 1+2 1-3 2+27 3+4 3-27 4+26 4-5 5+6 6+7 6-8 7+27 8+9 8-10 9+27 " +
        "10+11 10-12 11+27 12+13 12-14 13+27 14+15 14-16 15+27 16+17 16-18 17+27 " +
        "18+19 18-20 19+27 20+21 20-22 21+27 22+23 22-24 23+27 24+25 24-6 25+27 26+27]");

    public const string MultiBreakMap201Code = @"if (1) {
    2
    if (!(3)) {
        4
        loop {
            if (5) {
                6
                break
            }
            if (7) {
                8
                break
            }
            if (9) {
                10
                break
            }
            if (11) {
                12
                break
            }
        }
    }
}"; // 202 identical to 201
    public static ControlFlowGraph MultiBreakMap201 => ControlFlowGraph.FromString("[0,13,14,0+1 1+2 1-13 2+3 3+13 3-4 4+5 5+6 5-7 6+13 7+8 7-9 8+13 9+10 9-11 10+13 11+12 11-5 12+13]");
    public static ControlFlowGraph InfiniteLoopMap149 => ControlFlowGraph.FromString("[0,9,10,0+1 1+2 1-3 2+9 3+4 4+8 4-5 5+6 5-8 6+7 7+7 8-9]");
    public const string InfiniteLoopMap149Code = @"if (1) {
    2
} else {
    3
    if (4) {
        goto L1
    } else {
        if (5) {
            6
            loop {
                7
            }
        } else {
            L1:
            8
        }
    }
}";

    public const string LoopEdgeCaseMap174Code = "if (1) { loop { 2, if (3) { break }, 4 }, 6 } else { 5 }";
    public static ControlFlowGraph LoopEdgeCaseMap174 => ControlFlowGraph.FromString("[0,7,8,0+1 1+2 1-5 2+3 3+6 3-4 4+2 5+7 6+7]");

    public const string LoopEdgeCaseMap302Code = "if (1) { 2 } else { 3, loop { 4, if (5) { break }, 6 }, 7 }";
    public static ControlFlowGraph LoopEdgeCaseMap302 => ControlFlowGraph.FromString("[0,8,9,0+1 1+2 1-3 2+8 3+4 4+5 5+7 5-6 6+4 7+8]");

    public const string LoopEdgeCaseMap305Code = "if (1) { loop { 2, if (3) { break }, 4 }, 6 } else { 5 }, 7, loop { 8, if (9) { break }, 10 }";
    public static ControlFlowGraph LoopEdgeCaseMap305 => ControlFlowGraph.FromString("[0,11,12,0+1 1+2 1-5 2+3 3+6 3-4 4+2 5+7 6+7 7+8 8+9 9+11 9-10 10+8]");

    public const string LoopEdgeCaseMap305ReducedCode = "if (1) { loop { 2, if (3) { break }, 4 }, 6 } else { 5 }";
    public static ControlFlowGraph LoopEdgeCaseMap305Reduced => ControlFlowGraph.FromString("[0,7,8,0+1 1+2 1-5 2+3 3+6 3-4 4+2 5+7 6+7]");

    public const string LoopEdgeCaseMap313Code = "loop { 1, if (2) { break }, 3 }";
    public static ControlFlowGraph LoopEdgeCaseMap313 => ControlFlowGraph.FromString("[0,4,5,0+1 1+2 2+4 2-3 3+1]");
    /* 0
       |
    /->1-f-\
    |  |   |
    f  2   |
    |  |   |
    \--3-t\|
           4 */
    public const string LoopBreaksBothEndsCode = "while (1) { 2, if (3) { break } }";
    public static ControlFlowGraph LoopBreaksBothEnds => ControlFlowGraph.FromString("[0,4,5,0+1 1+2 2+3 3-1 1-4 3+4]");
}