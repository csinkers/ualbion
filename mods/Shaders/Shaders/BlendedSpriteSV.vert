#include "BlendedSpriteSV.h.vert"

void main()
{
	vec4 pos = vec4(iPosition, 0, 0);
	switch (iFlags & SF_ALIGNMENT_MASK)
	{
		case SF_MID_MID:     pos += vec4(0,    -0.5f, 0, 0); break;
		case SF_BOTTOM_MID:  pos += vec4(0,    -1.0f, 0, 0); break;
		case SF_TOP_LEFT:    pos += vec4(0.5f,    0,  0, 0); break;
		case SF_MID_LEFT:    pos += vec4(0.5f, -0.5f, 0, 0); break;
		case SF_BOTTOM_LEFT: pos += vec4(0.5f, -1.0f, 0, 0); break;
	};

    pos = pos * vec4(iSize.xyx, 1);

	if ((iFlags & SF_FLOOR) != 0)
		pos.xyzw = pos.xzyw;

    if ((iFlags & SF_BILLBOARD) != 0)
    {
		float rot = -uCameraLookDirection.y;
        float cx = cos(rot);
        float sx = sin(rot);

        pos = mat4(
             cx, 0, sx, 0,
              0, 1,  0, 0,
            -sx, 0, cx, 0,
              0, 0,  0, 1) * pos;
    }

	pos += iInstancePos;

    vec4 normPosition = ((uFlags & SKF_NO_TRANSFORM) == 0)
        ? uProjection * uView * pos
        : pos;

    gl_Position = normPosition;

    oTexPosition1 = iTexCoords * iTexSize1 + iTexOffset1;
    oTexPosition2 = iTexCoords * iTexSize2 + iTexOffset2;
    oLayer1 = float(iTexLayer1);
    oLayer2 = float(iTexLayer2);
    oFlags = iFlags;
    oNormCoords = iTexCoords;
    oWorldPosition = pos.xyz;
}
