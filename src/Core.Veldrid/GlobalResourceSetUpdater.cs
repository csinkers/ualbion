using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class GlobalResourceSetUpdater : Component
{
    readonly GlobalResourceSetProvider _globalSet;

    public GlobalResourceSetUpdater(GlobalResourceSetProvider globalSet)
    {
        _globalSet = globalSet;
        On<PrepareFrameEvent>(_ => UpdatePerFrameResources());
    }

    void UpdatePerFrameResources()
    {
        var clock = TryResolve<IClock>();
        var textureSource = Resolve<ITextureSource>();
        var paletteManager = Resolve<IPaletteManager>();
        var engineFlags = ReadVar(V.Core.User.EngineFlags);

        var dayPalette = textureSource.GetSimpleTexture(paletteManager.Day.Texture);
        var nightTexture = paletteManager.Night?.Texture ?? paletteManager.Day.Texture;
        var nightPalette = textureSource.GetSimpleTexture(nightTexture);

        _globalSet.DayPalette = dayPalette;
        _globalSet.NightPalette = nightPalette;
        _globalSet.GlobalInfo = new GlobalInfo
        {
            Time = clock?.ElapsedTime ?? 0,
            EngineFlags = engineFlags,
            PaletteBlend = paletteManager.Blend,
            PaletteFrame = paletteManager.Frame,
        };
    }
}