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
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion
{
    static class IsoBake
    {
        public static void Bake(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
#pragma warning disable CA2000 // Dispose objects before losing scopes
            var config = global.Resolve<IGeneralConfig>();
            var shaderCache = new ShaderCache(config.ResolvePath("$(CACHE)/ShaderCache"));

            foreach (var shaderPath in global.Resolve<IModApplier>().ShaderPaths)
                shaderCache.AddShaderPath(shaderPath);

            float isoYaw = ApiUtil.DegToRad(45);
            float isoPitch = ApiUtil.DegToRad(70);
            var isoAngles = new Vector2(isoYaw, isoPitch);

            using var engine = new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc, commandLine.StartupOnly) { WindowTitle = "UAlbion" }
                .AddRenderer(new ExtrudedTileMapRenderer(isoAngles));
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
                    .AddScene((Scene)new IsometricBakeScene().Add(new PaletteManager())))
                ;

            global.Raise(new SetSceneEvent(SceneId.IsometricBake), null);

            var assets = global.Resolve<IAssetManager>();
            var labId = (LabyrinthId)Base.LabyrinthData.Argim;
            var lab = assets.LoadLabyrinthData(labId);

            var tileSpacing = new Vector2(64, 64);
            float widthSkew = (float)Math.Cos(isoYaw);
            float heightSkew = (float)Math.Cos(isoPitch);
            float sideLength = tileSpacing.X * widthSkew;
            float longAxis = tileSpacing.Y * heightSkew;

            var tileSize = new Vector3(sideLength, longAxis, sideLength);
            var layout = new IsometricLayout(lab, Base.Palette.GlowyPlantDungeon, tileSize, tileSpacing);
            global.Attach(layout);

            global.Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.FlipDepthRange), null);
            global.Raise(new CameraMagnificationEvent(1), null);
            global.Raise(new CameraPlanesEvent(0, 16 * sideLength), null);
            global.Raise(new CameraPositionEvent(0, -4 * sideLength, 0), null);
            global.Raise(new CameraDirectionEvent(0, 90), null);

            engine.Run();
        }
    }
}