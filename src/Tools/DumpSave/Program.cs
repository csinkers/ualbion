using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace DumpSave;

static class Program
{
    class Command
    {
        public Command(string name, Action<EventExchange, string, TextWriter> action, string description)
        {
            Name = name;
            Action = action;
            Description = description;
        }

        public string Name { get; }
        public Action<EventExchange, string, TextWriter> Action { get; }
        public string Description { get; }
    }

    static readonly Command[] Commands =
    {
        new("dve", DumpVisitedEvents, "Dump Visited Events: Dump details of events and conversation paths that have been triggered."),
        new("dtc", DumpTempMapChanges, "Dump Temp Changes: Dump details of temporary changes to the current map."),
        new("dpc", DumpPermMapChanges, "Dump Perm Changes: Dump details of permanent changes to all maps."),
        new("a", DumpAnnotated, "Dump annotated")
    };

    static void DumpVisitedEvents(EventExchange exchange, string filename, TextWriter writer)
    {
        var save = VerifiedLoad(exchange, filename, writer);
        writer.WriteLine("VisitedEvents:");
        foreach (var e in save.VisitedEvents)
            writer.WriteLine(e);
    }

    static void DumpTempMapChanges(EventExchange exchange, string filename, TextWriter writer)
    {
        var save = VerifiedLoad(exchange, filename, writer);
        writer.WriteLine("Temp Map Changes:");
        foreach (var e in save.TemporaryMapChanges)
            writer.WriteLine(e);
    }

    static void DumpPermMapChanges(EventExchange exchange, string filename, TextWriter writer)
    {
        var save = VerifiedLoad(exchange, filename, writer);
        writer.WriteLine("Perm Map Changes:");
        foreach (var e in save.PermanentMapChanges)
            writer.WriteLine(e);
    }

    static void DumpAnnotated(EventExchange exchange, string filename, TextWriter writer)
    {
        var disk = exchange.Resolve<IFileSystem>();
        var stream = disk.OpenRead(filename);
        using var br = new BinaryReader(stream, Encoding.GetEncoding(850));
        var spellManager = exchange.Resolve<ISpellManager>();
        var s1 = new AlbionReader(br, stream.Length);

        using var ms = new MemoryStream();
        var s2 = new AnnotationProxySerializer(s1, writer, FormatUtil.BytesFrom850String);
        try
        {
            SavedGame.Serdes(null, AssetMapping.Global, s2, spellManager);
        }
        catch (Exception ex)
        {
            writer.WriteLine();
            writer.WriteLine($"Exception: {ex}");
        }
    }

    static bool VerifyRoundTrip(Stream fileStream, TextWriter writer, SavedGame save, AssetMapping mapping, ISpellManager spellManager)
    {
        using var ms = new MemoryStream((int)fileStream.Length);
        using var bw = new BinaryWriter(ms, Encoding.GetEncoding(850));
        SavedGame.Serdes(save, mapping, new AlbionWriter(bw), spellManager);

        if (ms.Position != fileStream.Length)
        {
            writer.WriteLine($"Assertion failed: Round-trip length mismatch (read {fileStream.Length}, wrote {ms.Position}");
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

            writer.WriteLine($"Assertion failed: Round-trip mismatch at {ms.Position:X}: read {a}, wrote {b}");
            errors++;
        }

        return errors == 0;
    }

    static SavedGame VerifiedLoad(EventExchange exchange, string filename, TextWriter writer)
    {
        var disk = exchange.Resolve<IFileSystem>();
        var stream = disk.OpenRead(filename);
        using var br = new BinaryReader(stream);

        var spellManager = exchange.Resolve<ISpellManager>();
        var save = SavedGame.Serdes(null, AssetMapping.Global, new AlbionReader(br, stream.Length), spellManager);

        if (!VerifyRoundTrip(stream, writer, save, AssetMapping.Global, spellManager))
            throw new InvalidOperationException("Saved-game round-tripping failed");

        return save;
    }

    static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
        var disk = new FileSystem(Directory.GetCurrentDirectory());
        var exchange = AssetSystem.SetupSimple(disk, AssetMapping.Global, "Base");

        var commands = ParseCommands(args.Skip(1)).ToList();
        if (!commands.Any())
        {
            PrintUsage();
            return;
        }

        var filename = args[0];
        var outPath = filename + ".txt";
        using var outputStream = File.Open(outPath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(outputStream);

        foreach (var command in commands)
        {
            command.Action(exchange, filename, writer);
            writer.WriteLine();
        }

        writer.Flush();
    }

    static void PrintUsage()
    {
        Console.WriteLine("UAlbion Save Dumping and Editing Utility");
        Console.WriteLine("Usage: DumpSave <SavePath> [Commands]");
        Console.WriteLine();
        Console.WriteLine("Valid commands:");
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