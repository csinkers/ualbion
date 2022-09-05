#include "MeshSF.h.frag"

void main()
{
	vec4 color = texture(sampler2D(Diffuse, Sampler), iTexCoords); //! vec4 color = vec4(0);
    oColor = color;
}

