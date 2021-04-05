using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Debugging;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Debugging;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    class IsoBake : Component
    {
        readonly IsometricLayout _layout;
        LabyrinthId _labId;
        float _pitch;
        float _yaw = 45;
        int _initialDiamondHeight = 40;
        int _width = 48;
        int _height = 64;
        int _tilesPerRow = 16;

        IsoBake()
        {
            _labId = Base.Labyrinth.Argim;
            _layout = AttachChild(new IsometricLayout());
            _pitch = ApiUtil.RadToDeg(MathF.Asin((float)_initialDiamondHeight / _width));

            On<IsoYawEvent>(e => { _yaw += e.Delta; Update(); });
            On<IsoPitchEvent>(e => { _pitch += e.Delta; Update(); });
            On<IsoRowWidthEvent>(e =>
            {
                _tilesPerRow += e.Delta;
                if (_tilesPerRow < 1) _tilesPerRow = 1;
                Update();
            });
            On<IsoWidthEvent>(e =>
            {
                _width += e.Delta;
                if (_width < 1) _width = 1;
                Update();
            });
            On<IsoHeightEvent>(e =>
            {
                _height += e.Delta;
                if (_height < 1) _height = 1;
                Update();
            });
            On<IsoLabEvent>(e =>
            {
                _labId = new LabyrinthId(AssetType.Labyrinth, _labId.Id + e.Delta);
                RecreateLayout();
            });
        }

        protected override void Subscribed()
        {
            Raise(new InputModeEvent(InputMode.IsoBake));
            Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.FlipDepthRange));
            Raise(new CameraMagnificationEvent(1.0f));
            Raise(new CameraPlanesEvent(0, 5000));
            // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
            RecreateLayout();
        }

        void RecreateLayout()
        {
            _layout.Load(_labId, BuildProperties(false));
            Update();
        }

        void Update() => _layout.Properties = BuildProperties();

        DungeonTileMapProperties BuildProperties(bool log = true)
        {
            _yaw = Math.Clamp(_yaw, -45.0f, 45.0f);
            _pitch = Math.Clamp(_pitch, 0, 85.0f);
            float yawRads = ApiUtil.DegToRad(_yaw);
            float pitchRads = ApiUtil.DegToRad(_pitch);

            float sideLength = _width * MathF.Cos(yawRads);
            float diamondHeight = _width * MathF.Sin(pitchRads);
            float height = (_height - diamondHeight) / MathF.Cos(pitchRads);

            int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
            if (log)
            {
                Raise(new LogEvent(LogEvent.Level.Info,
                    $"{_tilesPerRow}x{rows} " +
                    $"Y:{(int) _yaw} P:{(int) _pitch} " +
                    $"{_width}x{_height} = {sideLength:N2}x{height:N2} " +
                    $"DH:{diamondHeight:N2} R:{diamondHeight / _width:N2} " +
                    $"Total Dims: {_width * _tilesPerRow}x{_height * rows}"));
            }

            return new DungeonTileMapProperties(
                new Vector3(sideLength, height, sideLength),
                new Vector3(pitchRads, yawRads, 0),
                new Vector3(0, 0, 100), 
                _width * Vector3.UnitX,
                _height * Vector3.UnitY,
                (uint)_tilesPerRow,
                0,
                0);
        }

        public static void Bake(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
#pragma warning disable CA2000 // Dispose objects before losing scopes
            var config = global.Resolve<IGeneralConfig>();
            var shaderCache = new ShaderCache(config.ResolvePath("$(CACHE)/ShaderCache"));

            foreach (var shaderPath in global.Resolve<IModApplier>().ShaderPaths)
                shaderCache.AddShaderPath(shaderPath);

            using var engine = new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc, commandLine.StartupOnly)
            { WindowTitle = "UAlbion" }
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new DebugGuiRenderer());

            engine.ChangeBackend();
#pragma warning restore CA2000 // Dispose objects before losing scopes

            services
                .Add(shaderCache)
                .Add(engine)
                .Add(new DeviceObjectManager())
                .Add(new SpriteManager())
                .Add(new TextureManager())
                .Add(new SceneStack())
                .Add(new SceneManager()
                    .AddScene(new EmptyScene())
                    .AddScene((Scene)new IsometricBakeScene()
                        .Add(new PaletteManager())
                        .Add(new IsoBake())))
                .Add(new DebugMapInspector(services)
                    .AddBehaviour(new SpriteInstanceDataDebugBehaviour()))
                .Add(new InputManager().RegisterMouseMode(MouseMode.Normal, new NormalMouseMode()))
                .Add(new InputBinder(disk => InputConfig.Load(baseDir, disk)))
                ;

            global.Raise(new SetSceneEvent(SceneId.IsometricBake), null);
            engine.Run();
        }
    }
}