#include "FullscreenQuadSV.h.vert"

void main()
{
	vec2 pos = uRect.xy + uRect.zw * iPosition;
    gl_Position = vec4(pos, 0, 1);
	oNormCoords = vec2(iTexCoords.x, 1-iTexCoords.y);
}
