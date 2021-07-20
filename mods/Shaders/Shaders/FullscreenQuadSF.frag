#include "FullscreenQuadSF.h.frag"

void main()
{
    oColor = texture(sampler2D(uTexture, uSampler), iNormCoords); //! 
}
