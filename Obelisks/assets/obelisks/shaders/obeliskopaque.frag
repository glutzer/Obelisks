#version 330 core

in vec2 uv;
in vec4 colorOut;
in vec4 rgbaFog;
in float fogAmount;

uniform sampler2D tex2d; // Block atlas.
uniform float time;

#include noise3d.ash
#include fogandlight.fsh

out vec4 fragColor;

void main() {
  fragColor = texture(tex2d, uv) * colorOut;
  fragColor = applyFogAndShadow(fragColor, fogAmount);
}