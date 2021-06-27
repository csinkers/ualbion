#include "SpriteSV.h.vert"

void main()
{
    mat4 transform = mat4(
        vec4(iTransform1, 0),
        vec4(iTransform2, 0),
        vec4(iTransform3, 0),
        vec4(iTransform4, 1));

    if ((iFlags & SF_BILLBOARD) != 0)
    {
        float cx = cos(-uCameraLookDirection.x);
        float sx = sin(-uCameraLookDirection.x);

        transform = transform * mat4(
             cx, 0, sx, 0,
              0, 1,  0, 0,
            -sx, 0, cx, 0,
              0, 0,  0, 1);
    }

    vec4 worldSpace = transform * vec4(iPosition, 0, 1);

    vec4 normPosition = ((uFlags & SKF_NO_TRANSFORM) == 0)
        ? uProjection * uView * worldSpace
        : worldSpace;

    gl_Position = normPosition;

    oTexPosition = iTexCoords * iTexSize + iTexOffset;
    oLayer = float(iTexLayer);
    oFlags = iFlags;
    oNormCoords = iTexCoords;
    oWorldPosition = worldSpace.xyz;
}
