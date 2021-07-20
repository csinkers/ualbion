#include "FullscreenQuadSV.h.vert"

void main()
{
	vec2 pos = uRect.xy + uRect.zw * iPosition;
    gl_Position = vec4(pos, 0, 0);
	oNormCoords = iTexCoords;
}
