using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        public static EventExchange Global { get; private set; }
        static unsafe void Main()
        {
            //GraphTests();
            //return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core

            /*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

            Veldrid.Sdl2.SDL_version version;
            Veldrid.Sdl2.Sdl2Native.SDL_GetVersion(&version);

            var baseDir = Directory.GetParent(
                Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                .Location)) // ./ualbion/bin/Debug
                ?.Parent    // ./ualbion/bin
                ?.Parent    // ./ualbion
                ?.Parent    // .
                ?.FullName;

            if (string.IsNullOrEmpty(baseDir))
                return;

            using var assets = new AssetManager();
            var logger = new ConsoleLogger();
            Global = new EventExchange("Global", logger);
            Global
                // Need to register settings first, as the AssetConfigLocator relies on it.
                .Register<ISettings>(new Settings { BasePath = baseDir }) 
                .Register<IAssetManager>(assets)
                ;

            // Dump.CoreSprites(assets, baseDir);
            // Dump.CharacterSheets(assets);
            // Dump.Chests(assets);
            // Dump.ItemData(assets, baseDir);
            // Dump.MapEvents(assets, baseDir, MapDataId.Toronto2DGesamtkarteSpielbeginn);

            //return;

            RunGame(Global, baseDir);
        }

        static void RunGame(EventExchange global, string baseDir)
        {
            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            using var engine = new Engine(backend,
#if DEBUG
                true
#else
                false
#endif
                )
                .AddRenderer(new SpriteRenderer())
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new FullScreenQuad())
                .AddRenderer(new DebugGuiRenderer())
                .AddRenderer(new ScreenDuplicator())
                ;

            global
                .Register<IInputManager>(new InputManager()
                    .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                    .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                    .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                    .RegisterMouseMode(MouseMode.Exclusive, new ExclusiveMouseMode())
                    .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                    .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                )
                .Register<ILayoutManager>(new LayoutManager())
                .Register<IMapManager>(new MapManager())
                .Register<IPaletteManager>(new PaletteManager())
                .Register<ISceneManager>(new SceneManager()
                    .AddScene(new AutomapScene())
                    .AddScene(new FlatScene())
                    .AddScene(new DungeonScene())
                    .AddScene(new MenuScene())
                    .AddScene(new InventoryScene())
                )
                .Register<ISpriteResolver>(new SpriteResolver())
                .Register<IStateManager>(new StateManager())
                .Register<ITextManager>(new TextManager())
                .Register<ITextureManager>(new TextureManager())
                .Attach(engine)
                .Attach(new CursorManager())
                .Attach(new DebugMapInspector())
                .Attach(new GameClock())
                .Attach(new InputBinder(InputConfig.Load(baseDir)))
                .Attach(new InputModeStack())
                .Attach(new MouseModeStack())
                .Attach(new SceneStack())
                .Attach(new StatusBar())
                ;

            var inventoryConfig = InventoryConfig.Load(baseDir);
            global.Resolve<ISceneManager>().GetExchange(SceneId.Inventory)
                .Attach(new InventoryScreen(inventoryConfig))
                ;

            var menuBackground = new ScreenSpaceSprite<PictureId>(
                PictureId.MenuBackground8,
                new Vector2(0.0f, 1.0f),
                new Vector2(2.0f, -2.0f));

            global.Resolve<ISceneManager>().GetExchange(SceneId.MainMenu)
                .Attach(new MainMenu())
                .Attach(menuBackground)
                ;

            global.Raise(new NewGameEvent(), null);
            /*
            global.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
            global.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
            global.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/
            /*
            global.Raise(new SetSceneEvent(SceneId.Inventory), null);
            //*/

            // global.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
            ReflectionHelper.ClearTypeCache();
            engine.Run();
        }

        static string FormatChain(IEventNode node)
        {
            var graph = new ControlFlowGraph(node);
            var sb = new StringBuilder();
            FormatBlock(sb, graph.Start, 0);
            sb.AppendLine();
            return sb.ToString();
        }

        class Block
        {
            public override string ToString() => "{ " + string.Join("; ", Nodes) + " } ";
            public IList<IEventNode> Nodes { get; } = new List<IEventNode>();
            public IList<Block> Targets { get; } = new List<Block>();
            public IList<Block> Sources { get; } = new List<Block>();
            public void Add(IEventNode node) { Nodes.Add(node); }
        }

        class ControlFlowGraph
        {
            public ControlFlowGraph(IEventNode node)
            {
                var entryNode = new EventNode(-1, null) { NextEvent = node };
                var terminalNode = new EventNode(-2, null);
                //var leaders = new HashSet<IEventNode> { entryNode, node, terminalNode }; // Entry and exit nodes are always leaders
                //FindLeaders(entryNode, leaders, terminalNode);
                BuildBlocks(entryNode, terminalNode);//, leaders);
                LinkBlocks();
                Start = Blocks[entryNode];
                if (!Blocks.ContainsKey(terminalNode))
                    throw new InvalidOperationException("Invalid procedure: never exits");
                End = Blocks[terminalNode];
                CombineBlocks();
            }

            void CombineBlocks()
            {
                foreach(var node in Blocks.Keys.ToList())
                {
                    var block = Blocks[node];
                    if (block.Targets.Count == 1 && block.Targets[0].Sources.Count == 1)
                    {
                        var oldTarget = block.Targets[0];
                        if (block == Start || oldTarget == End)
                            continue;

                        block.Targets.Clear();
                        foreach (var target in oldTarget.Targets)
                        {
                            block.Targets.Add(target);
                            target.Sources.Remove(oldTarget);
                            target.Sources.Add(block);
                        }

                        foreach (var nodeInBlock in oldTarget.Nodes)
                        {
                            block.Nodes.Add(nodeInBlock);
                            Blocks[nodeInBlock] = block;
                        }
                    }
                }
            }

/*
            void FindLeaders(IEventNode node, ISet<IEventNode> leaders, IEventNode terminus)
            {
                if (node is IBranchNode branch)
                {
                    leaders.Add(node);
                    leaders.Add(branch.NextEvent);
                    leaders.Add(branch.NextEventWhenFalse);
                    if (branch.NextEventWhenFalse != null)
                        FindLeaders(branch.NextEventWhenFalse, leaders, terminus);
                    else
                        branch.NextEventWhenFalse = terminus;
                }

                if (node.NextEvent != null)
                    FindLeaders(node.NextEvent, leaders, terminus);
                else
                    node.NextEvent = terminus;
            }
*/
            void BuildBlocks(IEventNode node, IEventNode terminus)//, HashSet<IEventNode> leaders)
            {
                while (node != null)
                {
                    if (Blocks.ContainsKey(node))
                        return;

                    var block = new Block();
                    block.Add(node);
                    Blocks[node] = block;

                    if (node is IBranchNode branch)
                    {
                        if (branch.NextEventWhenFalse == null)
                            branch.NextEventWhenFalse = terminus;
                        BuildBlocks(branch.NextEventWhenFalse, terminus);
                    }

                    if (node.NextEvent == null && node != terminus)
                        node.NextEvent = terminus;

                    node = node.NextEvent;
                }
                /*
                while (node != null)
                {
                    if (Blocks.ContainsKey(node))
                        return;

                    var block = new Block();
                    while (node != null )
                    {
                        block.Add(node);
                        Blocks[node] = block;

                        if (node is IBranchNode branch)
                            BuildBlocks(branch.NextEventWhenFalse, leaders);

                        node = node.NextEvent;
                        if (leaders.Contains(node))
                            break;
                    }
                }
                */
            }

            void LinkBlocks()
            {
                foreach(var node in Blocks.Keys)
                {
                    if (node.NextEvent == null) // Terminus doesn't link to anything, skip.
                        continue;

                    var block = Blocks[node];
                    var nextBlock = Blocks[node.NextEvent];
                    if (nextBlock != block)
                    {
                        block.Targets.Add(nextBlock);
                        nextBlock.Sources.Add(block);
                    }

                    if (node is IBranchNode branch)
                    {
                        var falseBlock = Blocks[branch.NextEventWhenFalse];
                        block.Targets.Add(falseBlock);
                        falseBlock.Sources.Add(block);
                    }
                }
            }

            public IDictionary<IEventNode, Block> Blocks { get; } = new Dictionary<IEventNode, Block>();
            public Block Start { get; }
            public Block End { get; }
        }

        static void FormatBlock(StringBuilder sb, Block block, int level)
        {
            void Indent() => sb.Append("".PadLeft(level * 2));
            foreach (var node in block.Nodes)
            {
                Indent();
                sb.AppendLine(node.Event?.ToString());
            }

            foreach (var child in block.Targets)
                FormatBlock(sb, child, level + 1);

            // a -> b -> c
            // Block(a,b,c)

            // if(a) b; else c;
            // If(a, Block(b), Block(c))

            // if(a) b; c;
            // Block(If(a, Block(b)), c);

            // if(a) b; else { c; d; }
            // Block(If(a, Block(b), Block(c, d)));
        }

        class DummyEvent : IEvent
        {
            public DummyEvent(string name) { Name = name; }
            public string Name { get; }
            public override string ToString() => Name;
        }

        public class Chain
        {
            readonly IList<IEventNode> _nodes = new List<IEventNode>();

            public Chain Do(string name, ushort? next)
            {
                _nodes.Add(new EventNode(_nodes.Count, new DummyEvent(name)) { NextEventId = next });
                return this;
            }

            public Chain If(string name, ushort? ifTrue, ushort? ifFalse)
            {
                _nodes.Add(new BranchNode(_nodes.Count, new DummyEvent(name), ifFalse) { NextEventId = ifTrue });
                return this;
            }

            public IEventNode Build()
            {
                foreach(var node in _nodes)
                {
                    switch (node)
                    {
                        case BranchNode bn:
                            if (bn.NextEventId.HasValue)
                                bn.NextEvent = _nodes[bn.NextEventId.Value];
                            if (bn.NextEventWhenFalseId.HasValue)
                                bn.NextEventWhenFalse = _nodes[bn.NextEventWhenFalseId.Value];
                            break;

                        case EventNode en:
                            if (en.NextEventId.HasValue)
                                en.NextEvent = _nodes[en.NextEventId.Value];
                            break;
                    }
                }

                return _nodes[0];
            }
        }

        static void GraphTests()
        {
            // a
            var singleStatement = new Chain().Do("A", null).Build();
            Console.WriteLine(FormatChain(singleStatement));

            // a; b;
            var sequence = new Chain()
                .Do("A", 1)
                .Do("B", null).Build();
            Console.WriteLine(FormatChain(sequence));

            // if(a) { b; }
            var noFalse = new Chain()
                .If("A", 1, null)
                .Do("B", null).Build();
            Console.WriteLine(FormatChain(noFalse));

            // if(!a) { b; }
            var noTrue = new Chain()
                .If("A", null, 1)
                .Do("B", null).Build();
            Console.WriteLine(FormatChain(noTrue));

            // if(a) b; else c;
            var ifElse = new Chain()
                .If("A", 1, 2)
                .Do("B", null)
                .Do("C", null).Build();
            Console.WriteLine(FormatChain(ifElse));

            // if(a) { b } else { c } d;
            var diamond = new Chain()
                .If("A", 1, 2)
                .Do("B", 3)
                .Do("C", 3)
                .Do("D", null).Build();
            Console.WriteLine(FormatChain(diamond));

            // if(a) { b; } c;
            var aside = new Chain()
                .If("A", 1, 2)
                .Do("B", 2)
                .Do("C", null).Build();
            Console.WriteLine(FormatChain(aside));

            // if(a) { b; if(c) { d; return; } } e;
            var foo = new Chain()
                .If("A", 1, 4)
                .Do("B", 2)
                .If("C", 3, 4)
                .Do("D", null)
                .Do("E", null)
                .Build();
            Console.WriteLine(FormatChain(foo));

            // for(;;) A
            var infLoop = new Chain()
                .If("A", 1, null)
                .Do("B", 0)
                .Build();
            Console.WriteLine(FormatChain(infLoop));

            Console.ReadLine();
        }
    }
}
