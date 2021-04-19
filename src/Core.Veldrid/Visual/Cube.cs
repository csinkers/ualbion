using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public static class Cube
    {
        public static IndexFormat IndexFormat => IndexFormat.UInt16;
        public static VertexLayoutDescription VertexLayout => VertexLayoutHelper.Vertex3DTextured;

        public static readonly Vertex3DTextured[] Vertices = 
        { // Unit cube centred on the middle of the bottom face. Up axis = Y.
            // Floor (facing inward)
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 0.0f, 1.0f), //  0 Bottom Front Left
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), //  1 Bottom Front Right
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 0.0f, 0.0f), //  2 Bottom Back Left
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 1.0f, 0.0f), //  3 Bottom Back Right

            // Ceiling (facing inward)
            new Vertex3DTextured(-0.5f, 0.5f,  0.5f, 0.0f, 0.0f), //  4 Top Front Left
            new Vertex3DTextured( 0.5f, 0.5f,  0.5f, 1.0f, 0.0f), //  5 Top Front Right
            new Vertex3DTextured(-0.5f, 0.5f, -0.5f, 0.0f, 1.0f), //  6 Top Back Left
            new Vertex3DTextured( 0.5f, 0.5f, -0.5f, 1.0f, 1.0f), //  7 Top Back Right

            // Back (facing outward)
            new Vertex3DTextured(-0.5f,  0.5f, -0.5f, 1.0f, 0.0f), //  8 Back Top Right
            new Vertex3DTextured( 0.5f,  0.5f, -0.5f, 0.0f, 0.0f), //  9 Back Top Left
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 1.0f, 1.0f), // 10 Back Bottom Right
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 11 Back Bottom Left

            // Front (facing outward)
            new Vertex3DTextured( 0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 12 Front Top Left
            new Vertex3DTextured(-0.5f,  0.5f,  0.5f, 0.0f, 0.0f), // 13 Front Top Right
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 14 Front Bottom Left
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 0.0f, 1.0f), // 15 Front Bottom Right

            // Left (facing outward)
            new Vertex3DTextured(-0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 16 Back Top Left
            new Vertex3DTextured(-0.5f,  0.5f, -0.5f, 0.0f, 0.0f), // 17 Front Top Left
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 18 Back Bottom Left
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 19 Back Front Left

            // Right (facing outward)
            new Vertex3DTextured( 0.5f,  0.5f, -0.5f, 0.0f, 0.0f), // 20 Front Top Right
            new Vertex3DTextured( 0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 21 Back Top Right
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 22 Front Bottom Right
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 23 Back Bottom Right
        };

        public static readonly ushort[] Indices =
        {
            0,  1,  2,  2,  1,  3, // Floor
            6,  5,  4,  7,  5,  6, // Ceiling
            8,  9, 10, 10,  9, 11, // Back
            12, 13, 14, 14, 13, 15, // Front
            16, 17, 18, 18, 17, 19, // Left
            20, 21, 22, 22, 21, 23, // Right
        };
    }
}