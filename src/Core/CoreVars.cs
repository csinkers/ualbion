using UAlbion.Api.Settings;

namespace UAlbion.Core;

public class CoreVars
{
    CoreVars() { }
    public static VarLibrary Library { get; } = new();
    public static CoreVars Instance { get; } = new();

    public GfxVars Gfx { get; } = new();
    public class GfxVars : VarLibrary
    {
        public TextureSourceVars TextureSource { get; } = new();
        public class TextureSourceVars
        {
            public FloatVar CacheCheckIntervalSeconds { get; } = new(Library, "Core.Visual.TextureSource.CacheCheckIntervalSeconds", 5.0f);
        }

        public SpriteManagerVars SpriteManager { get; } = new();
        public class SpriteManagerVars
        {
            public FloatVar CacheCheckIntervalSeconds { get; } = new(Library, "Core.Visual.SpriteManager.CacheCheckIntervalSeconds", 12.0f);
        }

        public SkyboxVars Skybox { get; } = new();
        public class SkyboxVars
        {
            public FloatVar VisibleProportion { get; } = new(Library, "Core.Visual.Skybox.VisibleProportion", 0.8f);
        }
    }

    public UserVars User { get; } = new();
    public class UserVars
    {
        public FloatVar Special1 { get; } = new(Library, "Core.User.Special1", 0);
        public FloatVar Special2 { get; } = new(Library, "Core.User.Special2", 0);
        public CustomVar<EngineFlags, int> EngineFlags { get; } = new(Library, "Core.User.Visual.EngineFlags", Core.EngineFlags.VSync, x => (int)x, x => (EngineFlags)x, j => j.GetInt32());
    }

    public UiVars Ui { get; } = new();
    public class UiVars
    {
        public IntVar WindowPosX { get; } = new(Library, "Core.UI.Window.X", 0);
        public IntVar WindowPosY { get; } = new(Library, "Core.UI.Window.Y", 32);
        public IntVar WindowWidth { get; } = new(Library, "Core.UI.Window.W", 720);
        public IntVar WindowHeight { get; } = new(Library, "Core.UI.Window.H", 480);
        public StringVar ImGuiLayout { get; } = new(Library, "Core.UI.ImGuiLayout", "");
    }
}