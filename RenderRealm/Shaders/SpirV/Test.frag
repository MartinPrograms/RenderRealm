#version 450 core

layout (location = 0) out vec4 FragColor;

layout (location = 0) in vec2 TexCoord;

// Uniforms
layout (binding = 0, std140) uniform UBO {
    float Time;
};

void main() {
    FragColor = vec4(TexCoord, sin(Time), 1.0);
}