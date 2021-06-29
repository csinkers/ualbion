using System;
using System.IO;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;

namespace UAlbion.ShaderWriter
{
    class Program
    {
        const string AutoGenMessage = "This file was auto-generated using VeldridGen. It should not be edited by hand.";
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: UAlbion.ShaderWriter \"full path of shader directory\"");
                return 1;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Usage: UAlbion.ShaderWriter \"full path of shader directory\"");
                Console.WriteLine($"Directory \"{args[0]}\" not found");
                return 1;
            }

            bool success = true;
            success &= Emit(SpriteVertexShader.ShaderSource(), args[0]);
            success &= Emit(SpriteFragmentShader.ShaderSource(), args[0]);
            success &= Emit(EtmVertexShader.ShaderSource(), args[0]);
            success &= Emit(EtmFragmentShader.ShaderSource(), args[0]);
            success &= Emit(SkyboxVertexShader.ShaderSource(), args[0]);
            success &= Emit(SkyboxFragmentShader.ShaderSource(), args[0]);
            return success ? 0 : 1;
        }

        static bool Emit((string filename, string glsl) source, string directory)
        {
            var path = Path.Combine(directory, source.filename);
            if (File.Exists(path))
            {
                var currentText = File.ReadAllText(path);
                if (!currentText.Contains(AutoGenMessage))
                {
                    Console.WriteLine($"!! UAlbion.ShaderWriter: Tried to overwrite \"{path}\", but the file already exists and does not contain the expected VeldridGen auto-generation message.");
                    return false;
                }
            }

            File.WriteAllText(path, source.glsl);
            return true;
        }
    }
}
