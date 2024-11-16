﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public class SavedGameTests
{
    static void RoundTrip(string file)
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.Automap), AssetType.Automap)
            .RegisterAssetType(typeof(Base.Chest), AssetType.Chest)
            .RegisterAssetType(typeof(Base.EventSet), AssetType.EventSet)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            .RegisterAssetType(typeof(Base.NpcLargeGfx), AssetType.NpcLargeGfx)
            .RegisterAssetType(typeof(Base.PartyLargeGfx), AssetType.PartyLargeGfx)
            .RegisterAssetType(typeof(Base.Map), AssetType.Map)
            .RegisterAssetType(typeof(Base.Merchant), AssetType.Merchant)
            .RegisterAssetType(typeof(Base.NpcSheet), AssetType.NpcSheet)
            .RegisterAssetType(typeof(Base.PartySheet), AssetType.PartySheet)
            .RegisterAssetType(typeof(Base.Portrait), AssetType.Portrait)
            .RegisterAssetType(typeof(Base.NpcSmallGfx), AssetType.NpcSmallGfx)
            .RegisterAssetType(typeof(Base.PartySmallGfx), AssetType.PartySmallGfx)
            .RegisterAssetType(typeof(Base.Spell), AssetType.Spell)
            .RegisterAssetType(typeof(Base.Switch), AssetType.Switch)
            .RegisterAssetType(typeof(Base.Ticker), AssetType.Ticker);
        var mapping = AssetMapping.Global;
        var jsonUtil = new FormatJsonUtil();
        var spellManager = new MockSpellManager();
        foreach (var school in Enum.GetValues<SpellClass>())
        {
            for (byte i = 0; i < 30; i++)
            {
                var id = new SpellId((int)school * 30 + i);
                var spell = new SpellData(id, school, i);
                spellManager.Add(spell);
            }
        }

        // === Load ===
        using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(stream);
        using var annotationReadStream = new MemoryStream();
        using var annotationReader = new StreamWriter(annotationReadStream);
        using var ar = new AnnotationProxySerdes(new AlbionReader(br, stream.Length), annotationReader, FormatUtil.BytesFrom850String);
        var save = SavedGame.Serdes(null, mapping, ar, spellManager);

        // === Save ===
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var annotationWriteStream = new MemoryStream();
        using var annotationWriter = new StreamWriter(annotationWriteStream);
        using var aw = new AnnotationProxySerdes(new AlbionWriter(bw), annotationWriter, FormatUtil.BytesFrom850String);
        SavedGame.Serdes(save, mapping, aw, spellManager);

        File.WriteAllText(file + ".json", jsonUtil.Serialize(save));

        // write out debugging files and compare round-tripped data
        br.BaseStream.Position = 0;
        var originalBytes = br.ReadBytes((int)stream.Length);
        var roundTripBytes = ms.ToArray();

        ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Save game size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
        ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

        var diffs = XDelta.Compare(originalBytes, roundTripBytes).ToArray();
        if (diffs.Length != 1)
        {
            //* Save round-tripped and annotated text output for debugging

            static string ReadToEnd(Stream stream)
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream, null, true, -1, true);
                return reader.ReadToEnd();
            }

            ms.Position = 0;
            using var reloadBr = new BinaryReader(ms);
            using var reloadAnnotationStream = new MemoryStream();
            using var reloadAnnotationReader = new StreamWriter(reloadAnnotationStream);
            using var reloadFacade = new AnnotationProxySerdes(new AlbionReader(reloadBr, stream.Length), reloadAnnotationReader, FormatUtil.BytesFrom850String);
            SavedGame.Serdes(null, mapping, reloadFacade, spellManager);

            File.WriteAllBytes(file + ".bin", roundTripBytes);
            File.WriteAllText(file + ".pre.txt", ReadToEnd(annotationReadStream));
            File.WriteAllText(file + ".post.txt", ReadToEnd(annotationWriteStream));
            File.WriteAllText(file + ".reload.txt", ReadToEnd(reloadAnnotationStream));

            Console.WriteLine($"===== {file}.pre.txt =====");
            Console.WriteLine(File.ReadAllText($"{file}.pre.txt"));
            Console.WriteLine($"===== {file}.post.txt =====");
            Console.WriteLine(File.ReadAllText($"{file}.post.txt"));
            Console.WriteLine($"===== {file}.reload.txt =====");
            Console.WriteLine(File.ReadAllText($"{file}.reload.txt"));
            //*/

            //* Save JSON for debugging
            {
                File.WriteAllText(file + ".json", jsonUtil.Serialize(save));
                Console.WriteLine($"===== {file}.json =====");
                Console.WriteLine(File.ReadAllText($"{file}.json"));
            }
            //*/
        }

        Assert.Collection(diffs,
            d =>
            {
                Assert.True(d.IsCopy);
                Assert.Equal(0, d.Offset);
                Assert.Equal(originalBytes.Length, d.Length);
            });
    }

    [Fact]
    public void NewGameRoundTrip()
    {
        var disk = new MockFileSystem(true);
        var baseDir = ConfigUtil.FindBasePath(disk);
        RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "NewGame.001"));
    }

    [Fact]
    public void LateGameRoundTrip()
    {
        var disk = new MockFileSystem(true);
        var baseDir = ConfigUtil.FindBasePath(disk);
        RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "LateGame.001"));
    }
}
