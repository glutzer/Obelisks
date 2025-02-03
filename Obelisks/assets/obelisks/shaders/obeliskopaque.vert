#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec3 normalIn;

uniform mat4 modelMatrix;

// Glow value from 0-255;
uniform int glowAmount = 0;

uniform vec4 rgbaFogIn;
out vec4 rgbaFog;

uniform vec3 rgbaAmbientIn;
uniform float fogMinIn;
uniform float fogDensityIn;
out float fogAmount;

uniform vec4 blockLightIn;

#include vertexflagbits.ash
#include shadowcoords.vsh
#include fogandlight.vsh

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;
out vec4 colorOut;

void main() {
  gl_Position =
      perspectiveMatrix * viewMatrix * modelMatrix * vec4(vertexIn, 1.0);
  uv = uvIn;

  // Apply light.
  colorOut = applyLight(rgbaAmbientIn, blockLightIn, glowAmount,
                        modelMatrix * vec4(vertexIn, 1.0));

  rgbaFog = rgbaFogIn;
  fogAmount = getFogLevel(vec4(vertexIn, 0), fogMinIn, fogDensityIn);
}