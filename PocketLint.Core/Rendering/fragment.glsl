#version 330 core
in vec2 vTexCoord;
out vec4 FragColor;
uniform usampler2D indexTexture;
uniform sampler2D paletteTexture;
void main()
{
    uint index = texture(indexTexture, vTexCoord).r;
    if (index == 0u) discard; // Transparent
    float paletteCoord = (float(index) - 0.5) / 16.0; // Map index 1-16 to 0-1
    FragColor = texture(paletteTexture, vec2(paletteCoord, 0.0));
}