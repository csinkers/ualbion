﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid.Utilities;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Veldrid.Assets;

public class MeshLoader : IAssetLoader<Mesh>
{
    readonly PngDecoder _decoder = new();
    readonly Configuration _configuration = new();

    public Mesh Serdes(Mesh existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsReading())
            throw new NotSupportedException();

        var bytes = s.Bytes("MeshData", null, (int)s.BytesRemaining);
        using var objMs = new MemoryStream(bytes);

        ObjParser objParser = new();
        ObjFile obj = objParser.Parse(objMs);

        if (obj.MeshGroups.Length > 1)
            throw new NotSupportedException("Meshes with multiple mesh groups are not currently supported");

        var mesh = obj.GetFirstMesh();

        var materialPath = Path.Combine(context.Filename, obj.MaterialLibName);
        if (!context.Disk.FileExists(materialPath))
            throw new FileNotFoundException($"Could not find material file \"{materialPath}\" for object {context.AssetId} \"{context.Filename}\"");

        using var materialStream = context.Disk.OpenRead(materialPath);
        MtlParser mtlParser = new();
        MtlFile mtl = mtlParser.Parse(materialStream);

        if (!mtl.Definitions.TryGetValue(mesh.MaterialName, out var material))
            throw new ArgumentException($"Material \"{mesh.MaterialName}\" not found");

        var textures = new Dictionary<string, ITexture>();

        void AddTexture(string filename)
        {
            if (string.IsNullOrEmpty(filename) || textures.ContainsKey(filename))
                return;

            var texture = LoadTexture(context, filename, context.Disk);
            if (texture != null)
                textures[filename] = texture;
        }

        AddTexture(material.AmbientTexture);
        AddTexture(material.DiffuseTexture);
        AddTexture(material.SpecularColorTexture);
        AddTexture(material.SpecularHighlightTexture);
        AddTexture(material.AlphaMap);
        AddTexture(material.BumpMap);
        AddTexture(material.DisplacementMap);
        AddTexture(material.StencilDecalTexture);

        var key = new MeshId(context.AssetId);
        return new Mesh(key, mesh, material, textures);
    }

    ITexture LoadTexture(AssetLoadContext context, string filename, IFileSystem disk)
    {
        var path = Path.Combine(context.Filename, filename);
        if (!disk.FileExists(path))
            return null;

        using var stream = disk.OpenRead(path);
        var image = _decoder.Decode<Rgba32>(_configuration, stream);
        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        var span = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        var texture = new SimpleTexture<uint>(context.AssetId, context.AssetId.ToString(), image.Width, image.Height, span);
        texture.AddRegion(0, 0, image.Width, image.Height);
        return texture;
    }

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((Mesh)existing, s, context);
}

