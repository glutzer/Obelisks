#version 330 core

layout(location = 0) in vec3 vertexIn;
layout(location = 1) in vec2 uvIn;
layout(location = 2) in vec3 normalIn;

uniform mat4 modelMatrix;

layout(std140) uniform renderGlobals {
  mat4 viewMatrix;
  mat4 perspectiveMatrix;
  mat4 orthographicMatrix;
};

out vec2 uv;
out vec3 worldNormal;
out vec3 position;
out vec3 eyeVector;

void main() {
  vec4 worldPos = modelMatrix * vec4(vertexIn, 1.0);
  vec4 mvPosition = viewMatrix * worldPos;

  position = mvPosition.xyz;

  gl_Position = perspectiveMatrix * mvPosition;

  vec3 normal = normalIn;

  eyeVector = normalize(mvPosition.xyz); // Camera is at 0, so no need to
  // subtract the camera's position.

  worldNormal = normalize(mat3(viewMatrix * modelMatrix) * normal);
  if (dot(worldNormal, eyeVector) > 0)
    worldNormal = -worldNormal;

  uv = uvIn;
}