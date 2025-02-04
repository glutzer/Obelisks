#version 330 core

in vec2 uv;
in vec4 colorOut;
in vec4 rgbaFog;
in float fogAmount;

uniform sampler2D tex2d; // Block atlas.
uniform float time;
uniform vec4 color;
uniform vec4 atlasMap;

#include noise3d.ash
#include fogandlight.fsh

layout(location = 0) out vec4 outAccu;
layout(location = 1) out vec4 outReveal;
layout(location = 2) out vec4 outGlow;

float map(float value, float min1, float max1, float min2, float max2) {
  return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void drawPixel(vec4 colorA) {
  float weight =
      colorA.a *
      clamp(0.03 / (1e-5 + pow(gl_FragCoord.z / 200, 4.0)), 1e-2, 3e3);

  outAccu = vec4(colorA.rgb * colorA.a, colorA.a) * weight;

  outReveal.r = colorA.a;

  float glowing = 1.0;

  outGlow = vec4(glowing, 0, 0, colorA.a);
}

void main() {
  vec2 uvNew = uv;

  uvNew.x = map(uvNew.x, atlasMap.x, atlasMap.z, 0, 1);
  uvNew.y = map(uvNew.y, atlasMap.y, atlasMap.w, 0, 1);

  float noiseX = gnoise(vec3(uvNew.x * 4, uvNew.y * 4, time * 0.5));
  float noiseY = gnoise(vec3(uvNew.x * 4, uvNew.y * 4, 1000.0 + time * 0.5));
  float noiseA = gnoise(vec3(uvNew.x * 10, uvNew.y * 10, time * 0.25));

  uvNew.x += noiseX * 0.05;
  uvNew.y += noiseY * 0.05;

  uvNew = clamp(uvNew, 0, 1);

  vec4 colorA = colorOut * color * texture(tex2d, uvNew);
  colorA.a *= noiseA * 0.5 + 0.7;

  drawPixel(colorA);
}