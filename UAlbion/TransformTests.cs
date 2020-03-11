using System;
using System.Numerics;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion
{
    static class TransformTests
    {
        public static void Run()
        {
            Vector3 Transform(SpriteInstanceData instance, Vector3 vector)
            {
                var vec4 = new Vector4(vector, 1.0f);
                var m = instance.Transform;
                var transformed = Vector4.Transform(vec4, m);
                return new Vector3(transformed.X, transformed.Y, transformed.Z);
            }

            var origin = new Vector3();
            var oneX = new Vector3(1,0,0);
            var oneY = new Vector3(0,1,0);
            var oneZ = new Vector3(0,0,1);

            void Test(string name, SpriteInstanceData instance)
            {
                var origin2 = Transform(instance, origin);
                var x2 = Transform(instance, oneX);
                var y2 = Transform(instance, oneY);
                var z2 = Transform(instance, oneZ);
                Console.WriteLine(name + ":");
                Console.WriteLine($"  0: {origin2}");
                Console.WriteLine($"  X: {x2}");
                Console.WriteLine($"  Y: {y2}");
                Console.WriteLine($"  Z: {z2}");
            }

            SpriteInstanceData Make(Vector3 position, Vector2 size) =>
                SpriteInstanceData.TopLeft(
                    position, size,
                    new SubImage(Vector2.Zero, Vector2.One, Vector2.One, 0), 
                    0);


            Test("Neutral", Make(Vector3.Zero, Vector2.One));
            Test("+1X", Make(new Vector3(1,0,0), Vector2.One));
            Test("+1Y", Make(new Vector3(0,1,0), Vector2.One));
            Test("+1Z", Make(new Vector3(0,0,1), Vector2.One));
            Test("*2X", Make(Vector3.Zero, new Vector2(2, 1)));
            Test("*2Y", Make(Vector3.Zero, new Vector2(1, 2)));

            var x = Make(new Vector3(1, 0, 0), Vector2.One);
            x.OffsetBy(new Vector3(0,1,0));
            Test("+1X+1Y", x);

            Console.ReadLine();
        }
    }
}
