using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;

namespace DumpSave
{
    static class Program
    {
        class Command
        {
            public Command(string name, Action<SavedGame> action, string description)
            {
                Name = name;
                Action = action;
                Description = description;
            }

            public string Name { get; }
            public Action<SavedGame> Action { get; }
            public string Description { get; }
        }

        static readonly Command[] Commands =
        {
            new("dve", DumpVisitedEvents, "Dump Visited Events: Dump details of events and conversation paths that have been triggered."),
            new("dtc", DumpTempMapChanges, "Dump Temp Changes: Dump details of temporary changes to the current map."),
            new("dpc", DumpPermMapChanges, "Dump Perm Changes: Dump details of permanent changes to all maps."),
            new("dn", DumpNpcs, "Dump NPC states: Dump details of NPCs on the current map.")
        };

        static void DumpVisitedEvents(SavedGame save)
        {
            Console.WriteLine("VisitedEvents:");
            foreach (var e in save.VisitedEvents)
                Console.WriteLine(e);
        }

        static void DumpTempMapChanges(SavedGame save)
        {
            Console.WriteLine("Temp Map Changes:");
            foreach (var e in save.TemporaryMapChanges)
                Console.WriteLine(e);
        }

        static void DumpPermMapChanges(SavedGame save)
        {
            Console.WriteLine("Perm Map Changes:");
            foreach (var e in save.PermanentMapChanges)
                Console.WriteLine(e);
        }

        static void DumpNpcs(SavedGame save)
        {
            Console.WriteLine("NPCs:");
            foreach (var e in save.Npcs)
            {
                if (e.Id.IsNone)
                    continue;
                Console.WriteLine($"{e.Id} O:{e.SpriteOrGroup} ({e.X1}, {e.Y1}) ({e.X2}, {e.Y2}) ({e.X3}, {e.Y3}) ({e.X4}, {e.Y4})");
                ColorPrint(0x4, e.Unk4);
                ColorPrint(0x6, e.Unk6);
                ColorPrint(0x8, e.Unk8);
                ColorPrint(0x9, e.Unk9);
                Console.WriteLine();
                ColorPrint(0x11, e.Unk11);
                ColorPrint(0x13, e.Unk13);
                ColorPrint(0x15, e.Unk15);
                ColorPrint(0x17, e.Unk17);
                Console.WriteLine();
                ColorPrint(0x19, e.Unk19);
                ColorPrint(0x1B, e.Unk1B);
                ColorPrint(0x1D, e.Unk1D);
                ColorPrint(0x1F, e.Unk1F);
                Console.WriteLine();
                ColorPrint(0x20, e.Unk20);
                ColorPrint(0x21, e.Unk21);
                ColorPrint(0x23, e.Unk23);
                ColorPrint(0x25, e.Unk25);
                Console.WriteLine();
                ColorPrint(0x27, e.Unk27);
                ColorPrint(0x29, e.Unk29);
                ColorPrint(0x32, e.Unk32);
                ColorPrint(0x33, e.Unk33);
                Console.WriteLine();
                ColorPrint(0x34, e.Unk34);
                ColorPrint(0x36, e.Unk36);
                ColorPrint(0x38, e.Unk38);
                ColorPrint(0x3A, e.Unk3A);
                Console.WriteLine();
                ColorPrint(0x3C, e.Unk3C);
                ColorPrint(0x3E, e.Unk3E);
                ColorPrint(0x40, e.Unk40);
                ColorPrint(0x42, e.Unk42);
                Console.WriteLine();
                ColorPrint(0x4C, e.Unk4C);
                ColorPrint(0x4E, e.Unk4E);
                ColorPrint(0x50, e.Unk50);
                ColorPrint(0x51, e.Unk51);
                Console.WriteLine();
                ColorPrint(0x52, e.Unk52);
                ColorPrint(0x53, e.Unk53);
                ColorPrint(0x54, e.Unk54);
                ColorPrint(0x56, e.Unk56);
                Console.WriteLine();
                ColorPrint(0x58, e.Unk58);
                ColorPrint(0x5A, e.Unk5A);
                ColorPrint(0x5C, e.Unk5C);
                ColorPrint(0x5E, e.Unk5E);
                Console.WriteLine();
                ColorPrint(0x60, e.Unk60);
                ColorPrint(0x61, e.Unk61);
                ColorPrint(0x62, e.Unk62);
                ColorPrint(0x64, e.Unk64);
                Console.WriteLine();
                ColorPrint(0x65, e.Unk65);
                ColorPrint(0x66, e.Unk66);
                ColorPrint(0x68, e.Unk68);
                ColorPrint(0x6A, e.Unk6A);
                Console.WriteLine();
                ColorPrint(0x6C, e.Unk6C);
                ColorPrint(0x6E, e.Unk6E);
                ColorPrint(0x70, e.Unk70);
                ColorPrint(0x72, e.Unk72);
                Console.WriteLine();
                ColorPrint(0x74, e.Unk74);
                ColorPrint(0x76, e.Unk76);
                ColorPrint(0x78, e.Unk78);
                ColorPrint(0x7A, e.Unk7A);
                Console.WriteLine();
                ColorPrint(0x7C, e.Unk7C);
                ColorPrint(0x7E, e.Unk7E);
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        static bool VerifyRoundTrip(Stream fileStream, SavedGame save, AssetMapping mapping)
        {
            using var ms = new MemoryStream((int)fileStream.Length);
            using var bw = new BinaryWriter(ms, Encoding.GetEncoding(850));
            SavedGame.Serdes(save, mapping, new AlbionWriter(bw));

            if (ms.Position != fileStream.Length)
            {
                Console.WriteLine($"Assertion failed: Round-trip length mismatch (read {fileStream.Length}, wrote {ms.Position}");
                return false;
            }

            ms.Position = 0;
            fileStream.Position = 0;
            int errors = 0;
            const int maxErrors = 20;
            for (int i = 0; i < ms.Length && i < fileStream.Length && errors < maxErrors; i++)
            {
                var a = ms.ReadByte();
                var b = fileStream.ReadByte();
                if (a == b) 
                    continue;

                Console.WriteLine($"Assertion failed: Round-trip mismatch at {ms.Position:X}: read {a}, wrote {b}");
                errors++;
            }

            return errors == 0;
        }

        static void Main(string[] args)
        {
            var disk = new FileSystem();
            var baseDir = ConfigUtil.FindBasePath(disk);
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            var commands = ParseCommands(args.Skip(1)).ToList();
            if (!commands.Any())
            {
                PrintUsage();
                return;
            }

            var filename = args[0];
            var stream = disk.OpenRead(filename);
            using var br = new BinaryReader(stream, Encoding.GetEncoding(850));
            var generalConfig = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk);
            var settings = GeneralSettings.Load(generalConfig, disk);
            var settingsManager = new SettingsManager(settings);
            var assets = new AssetManager();
            var loaderRegistry = new AssetLoaderRegistry();
            var containerLoaderRegistry = new ContainerRegistry();
            var postProcessorRegistry = new PostProcessorRegistry();
            var modApplier = new ModApplier();

            var exchange = new EventExchange(new LogExchange());
            exchange
                .Attach(settingsManager)
                .Register<IGeneralConfig>(generalConfig)
                .Attach(loaderRegistry)
                .Attach(containerLoaderRegistry)
                .Attach(postProcessorRegistry)
                .Attach(modApplier)
                .Attach(assets)
                ;

            modApplier.LoadMods(generalConfig, settings.ActiveMods);
            var save = SavedGame.Serdes(null, AssetMapping.Global, new AlbionReader(br, stream.Length));

            if (!VerifyRoundTrip(stream, save, AssetMapping.Global))
                return;

            foreach (var command in commands)
            {
                command.Action(save);
                Console.WriteLine();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("UAlbion Save Dumping and Editing Utility");
            Console.WriteLine("Usage:");
            var longestCommand = Commands.Max(x => x.Name.Length);
            foreach(var command in Commands)
                Console.WriteLine($"    {command.Name.PadRight(longestCommand)}: {command.Description}");
        }

        static IEnumerable<Command> ParseCommands(IEnumerable<string> args)
        {
            foreach(var arg in args)
            {
                var command = Commands.FirstOrDefault(x => x.Name == arg);
                if (command != null)
                    yield return command;
            }
        }

        static void ColorPrint(int offset, long value)
        {
            Console.Write("\t{0:X2}:", offset);
            Console.ForegroundColor = value switch 
                {
                    // ConsoleColor.Black,
                    {} x when x < -0 => ConsoleColor.DarkBlue,
                    {} x when x < 0x10 => ConsoleColor.DarkCyan,
                    {} x when x < 0x20 => ConsoleColor.DarkGray,
                    {} x when x < 0x30 => ConsoleColor.DarkGreen,
                    {} x when x < 0x40 => ConsoleColor.DarkYellow,
                    {} x when x < 0x50 => ConsoleColor.DarkMagenta,
                    {} x when x < 0x60 => ConsoleColor.DarkRed,
                    {} x when x < 0x70 => ConsoleColor.Gray,
                    {} x when x < 0x80 => ConsoleColor.Red,
                    {} x when x < 0x90 => ConsoleColor.Magenta,
                    {} x when x < 0xA0 => ConsoleColor.Yellow,
                    {} x when x < 0xB0 => ConsoleColor.Green,
                    {} x when x < 0xC0 => ConsoleColor.Cyan,
                    {} x when x < 0xD0 => ConsoleColor.Blue,
                    _ => ConsoleColor.White,
                };
            Console.Write("{0:X2}", value);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
