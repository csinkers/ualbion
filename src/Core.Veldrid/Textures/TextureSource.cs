using System;
using System.Text;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using Veldrid;
using VeldridGen.Interfaces;
using static System.FormattableString;

namespace UAlbion.Core.Veldrid.Textures;

public class TextureSource : ServiceComponent<ITextureSource>, ITextureSource
{
    static void Checkerboard(Span<byte> span)
    {
        for (int j = 0; j < 64; j++)
            for (int i = 0; i < 64; i++)
                span[j * 64 + i] = (byte)(j >= 32 ^ i >= 32 ? 255 : 1);
    }

    static ITexture BuildDefaultArray()
    {
        var result = new ArrayTexture<byte>(null, 64, 64, 2);
        Checkerboard(result.GetMutableLayerBuffer(0).Buffer);
        Checkerboard(result.GetMutableLayerBuffer(1).Buffer);
        return result;
    }

    static ITexture BuildDefault()
    {
        var result = new SimpleTexture<byte>(null, 64, 64);
        Checkerboard( result.GetMutableLayerBuffer(0).Buffer);
        return result;
    }

    static readonly ITexture DefaultTexture = BuildDefault();
    static readonly ITexture DefaultArrayTexture = BuildDefaultArray();
    readonly TextureCache<Texture2DHolder> _simple = new(x => new Texture2DHolder(x.Name), CreateTexture, DefaultTexture);
    readonly TextureCache<Texture2DArrayHolder> _array = new(x => new Texture2DArrayHolder(x.Name), CreateTexture, DefaultArrayTexture);
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

    public ITextureHolder GetSimpleTexture(ITexture texture) => _simple.GetTextureHolder(texture);
    public ITextureArrayHolder GetArrayTexture(ITexture texture) => _array.GetTextureHolder(texture);
    public ITextureHolder GetDummySimpleTexture() => GetSimpleTexture(_dummySimple);
    public ITextureArrayHolder GetDummyArrayTexture() => GetArrayTexture(_dummyArray);

    string Stats()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Texture Statistics:");
        sb.AppendLine(Invariant($"    Total Time: {_totalTime} Last Cleanup: {_lastCleanup}"));

        sb.AppendLine("Simple textures:");
        _simple.DumpStats(sb);
        sb.AppendLine("Array textures:");
        _array.DumpStats(sb);

        return sb.ToString();
    }

    void OnUpdate(EngineUpdateEvent e)
    {
        _totalTime += e.DeltaSeconds;
        var texConfig = Resolve<ICoreConfigProvider>().Core.Visual.TextureManager;

        if (_totalTime - _lastCleanup <= texConfig.CacheCheckIntervalSeconds)
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

    static Texture CreateTexture(GraphicsDevice device, ITexture texture)
    {
        var deviceTexture = texture switch
        { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
            // TODO: Custom mip-mapping using nearest matches in the palette
            LazyTexture<byte> lazy8 => VeldridTexture.CreateLazy(device, TextureUsage.Sampled, lazy8),
            LazyTexture<uint> lazy32 => VeldridTexture.CreateLazy(device, TextureUsage.Sampled, lazy32),
            IReadOnlyTexture<byte> eightBit => VeldridTexture.Create(device, TextureUsage.Sampled, eightBit),
            IReadOnlyTexture<uint> trueColor => VeldridTexture.Create(device, TextureUsage.Sampled, trueColor),
            _ => throw new NotSupportedException($"Image format {texture.GetType().GetGenericArguments()[0].Name} not currently supported")
        };
        return deviceTexture;
    }
}