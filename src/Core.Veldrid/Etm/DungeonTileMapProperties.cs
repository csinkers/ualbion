using System.Numerics;
using System.Runtime.InteropServices;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    [StructLayout(LayoutKind.Sequential)]
    public partial struct DungeonTileMapProperties : IUniformFormat
    {
        [Uniform("uScale")]            	public Vector4 Scale { get; set; }
        [Uniform("uRotation")]         	public Vector4 Rotation { get; set; }
        [Uniform("uOrigin")]           	public Vector4 Origin { get; set; }
        [Uniform("uHorizontalSpacing")]	public Vector4 HorizontalSpacing { get; set; }
        [Uniform("uVerticalSpacing")]  	public Vector4 VerticalSpacing { get; set; }
        [Uniform("uWidth")]            	public uint Width { get; set; }
        [Uniform("uAmbient")]          	public uint AmbientLightLevel { get; set; }
        [Uniform("uFogColor")]         	public uint FogColor { get; set; }
        [Uniform("uYScale")]            public float ObjectYScaling { get; set; }

        public DungeonTileMapProperties(
            Vector3 scale,
            Vector3 rotation,
            Vector3 origin,
            Vector3 horizontalSpacing,
            Vector3 verticalSpacing,
            uint width,
            uint ambientLightLevel,
            uint fogColor,
            float objectYScaling)
        {
            Scale = new Vector4(scale, 1);
            Rotation = new Vector4(rotation, 1);
            Origin = new Vector4(origin, 1);
            HorizontalSpacing = new Vector4(horizontalSpacing, 1);
            VerticalSpacing = new Vector4(verticalSpacing, 1);
            Width = width;
            AmbientLightLevel = ambientLightLevel;
            FogColor = fogColor;
            ObjectYScaling = objectYScaling;
        }
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
