using UAlbion.Api.Settings;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Core;
public static class CoreVars
{
    public static class Gfx
    {
        public static class TextureSource
        {
            public static readonly FloatVar CacheCheckIntervalSeconds = new("Visual.TextureSource.CacheCheckIntervalSeconds", 5.0f);
        }

        public static class SpriteManager
        {
            public static readonly FloatVar CacheCheckIntervalSeconds = new("Visual.SpriteManager.CacheCheckIntervalSeconds", 12.0f);
        }

        public static class Skybox
        {
            public static readonly FloatVar VisibleProportion = new("Visual.Skybox.VisibleProportion", 0.8f);
        }
    }

    public static class User
    {
        public static readonly FloatVar Special1 = new("User.Special1", 0);
        public static readonly FloatVar Special2 = new("User.Special2", 0);
        public static readonly CustomVar<EngineFlags, int> EngineFlags = new("User.Visual.EngineFlags", Core.EngineFlags.VSync, x => (int)x, x => (EngineFlags)x, j => j.GetInt32());
    }
}
#pragma warning restore CA1034 // Nested types should not be visible