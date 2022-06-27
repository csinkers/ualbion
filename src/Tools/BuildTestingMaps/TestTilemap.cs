using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public class TestTilemap
{
    public ushort BlankOffset { get; }
    public ushort SolidOffset { get; }
    public ushort UnderlayOffset { get; }

    public ushort OverlayOffset { get; }

    // public int FlagTileOffset { get; }
    public ushort SitOffset { get; }
    public ushort TextOffset { get; }
    public ushort AnimLoopOffset { get; }
    public ushort AnimCycleOffset { get; }
    public ushort AnimLoopOverlayOffset { get; }
    public ushort AnimCycleOverlayOffset { get; }
    public TilesetData Tileset { get; }
    public Dictionary<AssetId, object> Assets { get; }= new();

    public ushort IndexForChar(char c)
    {
        if (c is < ' ' or > '~') return 0;
        return (ushort)((c - ' ') + TextOffset);
    }

    static IReadOnlyTexture<byte> MakeTileGfx(bool overlay, byte num, ITextureBuilderFont font)
    {
        var t = T16(null).FillRect(overlay ? CBlue2 : CGrey6, 0, 0, TileWidth, overlay ? TileHeight / 2 : TileHeight);

        if (!overlay)
            t = t.Border(CGreen4);

        return
            t.Text($"{num:X2}", CWhite, 2, overlay ? 2 : 9, font)
                .Texture;
    }

    public TestTilemap(ITextureBuilderFont font, ITextureBuilderFont bigFont)
    {
        var tiles = new List<IReadOnlyTexture<byte>>
        {
            T16(null).FillAll(CBlack1).Texture,
            T16(null).FillAll(CBlack1).Border(CWhite).Texture,
            T16(null).FillAll(CGrey12).Texture,
        };

        Tileset = new TilesetData(UAlbion.Base.Tileset.Toronto) { UseSmallGraphics = false };
        Tileset.Tiles.Add(new(Tileset.Tiles.Count, 1, TileType.Unk0, TileLayer.Normal));
        BlankOffset = (ushort)Tileset.Tiles.Count;
        Tileset.Tiles.Add(new(Tileset.Tiles.Count, 1, TileType.Unk0, TileLayer.Normal));
        SolidOffset = (ushort)Tileset.Tiles.Count;
        Tileset.Tiles.Add(new(Tileset.Tiles.Count, 2, TileType.Unk0, TileLayer.Normal)
            { Collision = Passability.Solid });

        UnderlayOffset = (ushort)Tileset.Tiles.Count;
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int gfxIndex = tiles.Count;
                tiles.Add(MakeTileGfx(false, (byte)(i * 16 + j), font));
                Tileset.Tiles.Add(new(Tileset.Tiles.Count, (ushort)gfxIndex, (TileType)i, (TileLayer)j));
            }
        }

        OverlayOffset = (ushort)Tileset.Tiles.Count;
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int gfxIndex = tiles.Count;
                tiles.Add(MakeTileGfx(true, (byte)(i * 16 + j), font));
                Tileset.Tiles.Add(new(Tileset.Tiles.Count, (ushort)gfxIndex, (TileType)i, (TileLayer)j));
            }
        }

        SitOffset = (ushort)Tileset.Tiles.Count;
        for (int i = 0; i < 16; i++)
        {
            int gfxIndex = tiles.Count;
            tiles.Add(T16(null).FillAll(CGrey4)
                .Border(CWhite)
                .Text("S", CGreen5, 2, 2, font)
                .Text(i.ToString(), CGreen5, 2, 9, font)
                .Texture);

            Tileset.Tiles.Add(new(Tileset.Tiles.Count, (ushort)gfxIndex, 0, 0) { SitMode = (SitMode)i });
        }

        TextOffset = (ushort)Tileset.Tiles.Count;
        for (char c = ' '; c <= '~'; c++)
        {
            int gfxIndex = tiles.Count;
            tiles.Add(T16(null).FillAll(CBlueGrey7)
                .Border(COrange3)
                .Text(c == '`' ? "'" : c.ToString(), CGreen5, 2, 2, bigFont)
                .Texture);

            Tileset.Tiles.Add(new(Tileset.Tiles.Count, (ushort)gfxIndex, 0, 0));
        }

        const int loopFrameCount = 8;
        void BuildCycle(int num, int frameCount, Action<TileData, int> func)
        {
            for (int i = 0; i < num; i++)
            {
                int gfxIndex = tiles.Count;
                for (int j = 0; j < frameCount; j++)
                {
                    float t = (float)j / (frameCount - 1);
                    tiles.Add(T16(null).FillAll(CRainbowLoop[j % 8])
                        //.FillRect(CGrey8, 0, 0, 15, (int)(15 * t))
                        .Border(CWhite)
                        .Text(i.ToString("X") + j, CWhite, 2, 2, font)
                        .Texture);
                }

                var tile = new TileData(Tileset.Tiles.Count, (ushort)gfxIndex, 0, 0) { FrameCount = loopFrameCount, };
                func(tile, i);
                Tileset.Tiles.Add(tile);
            }
        }

        AnimLoopOffset = (ushort)Tileset.Tiles.Count;
        BuildCycle(8, loopFrameCount, (x,i) => x.Type = (TileType)i);

        AnimCycleOffset = (ushort)Tileset.Tiles.Count;
        BuildCycle(8, loopFrameCount, (x,i) => { x.Type = (TileType)i; x.Bouncy = true; });

        AnimLoopOverlayOffset = (ushort)Tileset.Tiles.Count;
        BuildCycle(8, loopFrameCount, (x,i) =>
        {
            x.Type = (TileType)i;
            x.UseUnderlayFlags = true;
        });

        AnimCycleOverlayOffset = (ushort)Tileset.Tiles.Count;
        BuildCycle(8, loopFrameCount, (x, i) =>
        {
            x.Type = (TileType)i;
            x.Bouncy = true;
            x.UseUnderlayFlags = true;
        });

        Unk7Type0Offset = (ushort)Tileset.Tiles.Count;
        BuildCycle(8, loopFrameCount, (x, i) =>
        {
            // Type1 = unk
            // Type2 = add (x - y) to starting frame
            // Type4 = crazy pattern
            x.Type = TileType.Unk3;
            x.Unk7 = (byte)i;
            x.Bouncy = true;
        });

        var gfxId = (SpriteId)UAlbion.Base.TilesetGfx.Toronto;
        Assets[gfxId] = new SimpleTileGraphics(BlitUtil.CombineFramesVertically(gfxId, tiles));
        Assets[Tileset.Id] = Tileset;
    }

    public ushort Unk7Type0Offset { get; }
}