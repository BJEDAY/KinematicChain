#version 400

out vec4 outputColor;

in vec2 texCoord;

// A sampler2d is the representation of a texture in a shader.
// Each sampler is bound to a texture unit.
// By default, the unit is 0, so no code-related setup is actually needed.
uniform sampler2D texture0;

void main()
{
    // To use a texture, you call the texture() function.
    // It takes two parameters: the sampler to use, and a vec2, used as texture coordinates.
    outputColor = texture(texture0, texCoord);
}