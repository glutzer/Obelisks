#version 330 core

uniform sampler2D tex2d;
uniform sampler2D tex2dAdd;

in vec2 texCoord;
out vec4 outColor;

void main() {
  outColor = texture(tex2d, texCoord);
  outColor.rgb += texture(tex2dAdd, texCoord).rgb * 0.5;
}