using OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public enum DistanceModel
{
    /// <summary>
    /// gain = RefDist / (RefDist + RollOffFactor * (distance - RefDist))
    /// </summary>
    InverseDistance = AL10.AL_INVERSE_DISTANCE,
    InverseDistanceClamped = AL10.AL_INVERSE_DISTANCE_CLAMPED,
    /*
    LinearDistance,
    LinearDistanceClamped,
    ExponentDistance,
    ExponentDistanceClamped,
    */
    None = AL10.AL_NONE
}