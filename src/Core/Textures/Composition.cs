using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures;

public class Composition
{
    readonly int[] _frameCounts;
    readonly IDictionary<LayerKey, int> _layerLookup;

    public Composition(ITexture texture, IDictionary<LayerKey, int> layerLookup, int[] frameCounts)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _layerLookup = layerLookup ?? throw new ArgumentNullException(nameof(layerLookup));
        _frameCounts = frameCounts ?? throw new ArgumentNullException(nameof(frameCounts));
    }

    public ITexture Texture { get; }

    public int GetFrameCountForLogicalId(int logicalId) 
        => logicalId >= _frameCounts.Length 
            ? 1 
            : _frameCounts[logicalId];

    public int GetSubImageAtTime(int logicalId, int tick, bool backAndForth)
    {
        if (logicalId >= _frameCounts.Length)
            return 0;

        var logicalImage = _frameCounts[logicalId];
        int frame;
        if (backAndForth && logicalImage > 2)
        {
            int maxFrame = logicalImage - 1;
            frame = tick % (2 * maxFrame) - maxFrame;
            frame = Math.Abs(frame);
        }
        else frame = tick % logicalImage;

        return _layerLookup.TryGetValue(new LayerKey(logicalId, frame), out var result) ? result : 0;
    }
}