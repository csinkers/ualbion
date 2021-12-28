using System;
using System.Text;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Textures;

public class TextureSource : ServiceComponent<ITextureSource>, ITextureSource
{
    readonly TextureCache<Texture2DHolder> _simple = new(x => new Texture2DHolder(x.Name), CreateSimple);
    readonly TextureCache<Texture2DArrayHolder> _array = new(x => new Texture2DArrayHolder(x.Name), CreateArray);
    readonly ITexture _dummySimple = new SimpleTexture<byte>(null, "Dummy Texture", 1, 1, new byte[] { 0 });
    readonly ITexture _dummyArray = new ArrayTexture<byte>(null, "Dummy ArrayTexture", 1, 1, 2, new byte[] { 0, 0 });
    float _lastCleanup;
    float _totalTime;

    public TextureSource()
    {
        AttachChild(_simple);
        AttachChild(_array);
        On<EngineUpdateEvent>(OnUpdate);
        On<TextureStatsEvent>(_ => Info(Stats()));
    }

    public ITextureHolder GetSimpleTexture(ITexture texture, int version = 0) => _simple.GetTextureHolder(texture, version);
    public ITextureArrayHolder GetArrayTexture(ITexture texture, int version = 0) => _array.GetTextureHolder(texture, version);
    public ITextureHolder GetDummySimpleTexture() => GetSimpleTexture(_dummySimple);
    public ITextureArrayHolder GetDummyArrayTexture() => GetArrayTexture(_dummyArray);

    string Stats()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Texture Statistics:");
        sb.AppendLine($"    Total Time: {_totalTime} Last Cleanup: {_lastCleanup}");

        sb.AppendLine("Simple textures:");
        _simple.DumpStats(sb);
        sb.AppendLine("Array textures:");
        _array.DumpStats(sb);

        return sb.ToString();
    }

    void OnUpdate(EngineUpdateEvent e)
    {
        _totalTime += e.DeltaSeconds;
        var config = Resolve<CoreConfig>().Visual.TextureManager;

        if (_totalTime - _lastCleanup <= config.CacheCheckIntervalSeconds)
            return;

        _simple.Cleanup();
        _array.Cleanup();
        _lastCleanup = _totalTime;
    }

    protected override void Unsubscribed()
    {
        _simple.Dispose();
        _array.Dispose();
    }

    static Texture CreateSimple(GraphicsDevice device, ITexture texture)
    {
        var deviceTexture = texture switch
        { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
            // TODO: Custom mip-mapping using nearest matches in the palette
            IReadOnlyTexture<byte> eightBit => VeldridTexture.CreateSimpleTexture(device, TextureUsage.Sampled, eightBit),
            IReadOnlyTexture<uint> trueColor => VeldridTexture.CreateSimpleTexture(
                device,
                TextureUsage.Sampled, // | ((trueColor.Height == 1) ? 0 : TextureUsage.GenerateMipmaps),
                trueColor),
            _ => throw new NotSupportedException($"Image format {texture.GetType().GetGenericArguments()[0].Name} not currently supported")
        };
        return deviceTexture;
    }

    static Texture CreateArray(GraphicsDevice device, ITexture texture) =>
        texture switch
        { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
            // TODO: Custom mip-mapping using nearest matches in the palette
            IReadOnlyTexture<byte> eightBitArray => 
                VeldridTexture.CreateArrayTexture(
                    device,
                    TextureUsage.Sampled,
                    eightBitArray),

            // Note: Veldrid's auto-generation of mip-maps looks to
            // be broken in Vulkan at the moment, so disabled for now.
            IReadOnlyTexture<uint> trueColorArray =>
                VeldridTexture.CreateArrayTexture(
                    device,
                    TextureUsage.Sampled,// | TextureUsage.GenerateMipmaps,
                    trueColorArray),

            _ => throw new NotSupportedException(
                $"Image format {texture.GetType().GetGenericArguments()[0].Name}" +
                " not currently supported")
        };
}