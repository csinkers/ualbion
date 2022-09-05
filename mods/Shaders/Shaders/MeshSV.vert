#include "MeshSV.h.vert"

void main()
{
    vec3 pos = iInstancePos + iPosition * iInstanceScale;
    vec4 normPosition = uProjection * uView * vec4(pos, 1.0f);

    gl_Position = normPosition;
    oTexCoords = iTexCoords;
}
