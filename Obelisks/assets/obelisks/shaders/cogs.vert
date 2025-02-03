#version 330 core
#extension GL_ARB_explicit_attrib_location : enable
#extension GL_ARB_shading_language_420pack : require

layout(location = 0) in vec3 vertex;
layout(location = 1) in vec2 uvIn;

out vec2 uv2;

uniform mat4 modelMatrix;

layout(std140, binding = 1) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

void main() {
  gl_Position = orthographicMatrix * modelMatrix * vec4(vertex, 1.0);
  uv2 = uvIn;
}