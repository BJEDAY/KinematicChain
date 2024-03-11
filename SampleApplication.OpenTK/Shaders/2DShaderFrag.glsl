#version 400 core
out vec4 FragColor;
in vec3 color;

void main()
{
    FragColor = vec4(color, 1.0f);
    //FragColor = vec4(1.0f,0.0f,1.0f, 1.0f);
}