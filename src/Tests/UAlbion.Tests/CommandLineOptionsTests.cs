using System;
using System.Collections.Generic;
using UAlbion.Config;
using Veldrid;
using Xunit;

namespace UAlbion.Tests
{
    public class CommandLineOptionsTests
    {
        [Fact]
        public void DefaultInitializationTest()
        {
            var options = new CommandLineOptions([]);
            Assert.Equal(ExecutionMode.Game, options.Mode);
            Assert.Equal(GraphicsBackend.Vulkan, options.Backend);
            Assert.False(options.DebugMenus);
            Assert.False(options.Mute);
            Assert.False(options.StartupOnly);
            Assert.False(options.UseRenderDoc);
            Assert.Null(options.ConvertFrom);
            Assert.Null(options.ConvertTo);
            Assert.Null(options.ConvertFilePattern);
            Assert.Null(options.Commands);
            Assert.Null(options.DumpIds);
            Assert.Null(options.Mods);
            Assert.Equal(DumpFormats.Json, options.DumpFormats);
            Assert.Null(options.DumpAssetTypes);
            Assert.Null(options.DumpLanguages);
        }

        [Fact]
        public void GameModeTest()
        {
            var options = new CommandLineOptions(["--game"]);
            Assert.Equal(ExecutionMode.Game, options.Mode);
        }

        [Fact]
        public void DumpModeTest()
        {
            var options = new CommandLineOptions(["--dump"]);
            Assert.Equal(ExecutionMode.DumpData, options.Mode);
        }

        [Fact]
        public void DumpModeTest2()
        {
            var options = new CommandLineOptions(["-d"]);
            Assert.Equal(ExecutionMode.DumpData, options.Mode);
        }

        [Fact]
        public void IsoModeTest()
        {
            var options = new CommandLineOptions(["--iso"]);
            Assert.Equal(ExecutionMode.BakeIsometric, options.Mode);
        }

        [Fact]
        public void ConvertModeTest()
        {
            var options = new CommandLineOptions(["--convert", "ModA", "ModB"]);
            Assert.Equal(ExecutionMode.ConvertAssets, options.Mode);
            Assert.Equal(new[] { "ModA" }, options.ConvertFrom);
            Assert.Equal("ModB", options.ConvertTo);
        }

        [Fact]
        public void ConvertModeTest2()
        {
            var options = new CommandLineOptions(["-b", "ModA ModB", "ModC"]);
            Assert.Equal(ExecutionMode.ConvertAssets, options.Mode);
            Assert.Equal(new[] { "ModA", "ModB" }, options.ConvertFrom);
            Assert.Equal("ModC", options.ConvertTo);
        }

        [Fact]
        public void Direct3DBackendTest()
        {
            var options = new CommandLineOptions(["--direct3d"]);
            Assert.Equal(GraphicsBackend.Direct3D11, options.Backend);
        }

        [Fact]
        public void Direct3DBackendTest2()
        {
            var options = new CommandLineOptions(["-d3d"]);
            Assert.Equal(GraphicsBackend.Direct3D11, options.Backend);
        }

        [Fact]
        public void OpenGLBackendTest()
        {
            var options = new CommandLineOptions(["--opengl"]);
            Assert.Equal(GraphicsBackend.OpenGL, options.Backend);
        }

        [Fact]
        public void OpenGLBackendTest2()
        {
            var options = new CommandLineOptions(["-gl"]);
            Assert.Equal(GraphicsBackend.OpenGL, options.Backend);
        }

        [Fact]
        public void OpenGLESBackendTest()
        {
            var options = new CommandLineOptions(["--opengles"]);
            Assert.Equal(GraphicsBackend.OpenGLES, options.Backend);
        }

        [Fact]
        public void OpenGLESBackendTest2()
        {
            var options = new CommandLineOptions(["-gles"]);
            Assert.Equal(GraphicsBackend.OpenGLES, options.Backend);
        }

        [Fact]
        public void MetalBackendTest()
        {
            var options = new CommandLineOptions(["--metal"]);
            Assert.Equal(GraphicsBackend.Metal, options.Backend);
        }

        [Fact]
        public void VulkanBackendTest()
        {
            var options = new CommandLineOptions(["--vulkan"]);
            Assert.Equal(GraphicsBackend.Vulkan, options.Backend);
        }

        [Fact]
        public void VulkanBackendTest2()
        {
            var options = new CommandLineOptions(["-vk"]);
            Assert.Equal(GraphicsBackend.Vulkan, options.Backend);
        }

        [Fact]
        public void DebugMenusTest()
        {
            var options = new CommandLineOptions(["--menus"]);
            Assert.True(options.DebugMenus);
        }

        [Fact]
        public void MuteTest()
        {
            var options = new CommandLineOptions(["--mute"]);
            Assert.True(options.Mute);
        }

        [Fact]
        public void StartupOnlyTest()
        {
            var options = new CommandLineOptions(["--startuponly"]);
            Assert.True(options.StartupOnly);
        }

        [Fact]
        public void RenderDocTest()
        {
            var options = new CommandLineOptions(["--renderdoc"]);
            Assert.True(options.UseRenderDoc);
        }

        [Fact]
        public void RenderDocTest2()
        {
            var options = new CommandLineOptions(["-rd"]);
            Assert.True(options.UseRenderDoc);
        }

        [Fact]
        public void CommandsTest()
        {
            var options = new CommandLineOptions(["--commands", "cmd1;cmd2"]);
            Assert.Equal(new[] { "cmd1", "cmd2" }, options.Commands);
        }

        [Fact]
        public void FormatsTest()
        {
            var options = new CommandLineOptions(["--formats", "Json Text"]);
            Assert.Equal(DumpFormats.Json | DumpFormats.Text, options.DumpFormats);
        }

        [Fact]
        public void IdsTest()
        {
            var options = new CommandLineOptions(["--ids", "id1 id2"]);
            Assert.Equal(new[] { "id1", "id2" }, options.DumpIds);
        }

        [Fact]
        public void TypeTest()
        {
            var options = new CommandLineOptions(["--type", "ItemGfx Map"]);
            Assert.Contains(AssetType.ItemGfx, options.DumpAssetTypes);
            Assert.Contains(AssetType.Map, options.DumpAssetTypes);
        }

        [Fact]
        public void FilesTest()
        {
            var options = new CommandLineOptions(["--files", ".*\\.txt"]);
            Assert.Equal(".*\\.txt", options.ConvertFilePattern.ToString());
        }

        [Fact]
        public void ModsTest()
        {
            var options = new CommandLineOptions(["--mods", "Mod1 Mod2"]);
            Assert.Equal(["Mod1", "Mod2"], options.Mods);
        }
    }
}

