using UAlbion.Api.Settings;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Core;
public static class CoreVars
{
    public static class Gfx
    {
        public static class TextureSource
        {
            public static readonly FloatVar CacheCheckIntervalSeconds = new("Core.Visual.TextureSource.CacheCheckIntervalSeconds", 5.0f);
        }

        public static class SpriteManager
        {
            public static readonly FloatVar CacheCheckIntervalSeconds = new("Core.Visual.SpriteManager.CacheCheckIntervalSeconds", 12.0f);
        }

        public static class Skybox
        {
            public static readonly FloatVar VisibleProportion = new("Core.Visual.Skybox.VisibleProportion", 0.8f);
        }
    }

    public static class User
    {
        public static readonly FloatVar Special1 = new("Core.User.Special1", 0);
        public static readonly FloatVar Special2 = new("Core.User.Special2", 0);
        public static readonly CustomVar<EngineFlags, int> EngineFlags = new("Core.User.Visual.EngineFlags", Core.EngineFlags.VSync, x => (int)x, x => (EngineFlags)x, j => j.GetInt32());
    }

    public static class Ui
    {
        public static readonly IntVar WindowPosX     = new("Core.UI.Window.X", 0);
        public static readonly IntVar WindowPosY     = new("Core.UI.Window.Y", 32);
        public static readonly IntVar WindowWidth    = new("Core.UI.Window.W", 720);
        public static readonly IntVar WindowHeight   = new("Core.UI.Window.H", 480);
        public static readonly StringVar ImGuiLayout = new("Core.UI.ImGuiLayout", "");
    }
}
#pragma warning restore CA1034 // Nested types should not be visible