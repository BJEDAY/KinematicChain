#version 400 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec3 aColor;

uniform mat4 persp;
uniform mat4 view;
uniform mat4 model;

out vec3 color;

void main()
{
    color = aColor;
    gl_Position = vec4(aPosition,0.0f, 1.0) * model * view * persp;
}