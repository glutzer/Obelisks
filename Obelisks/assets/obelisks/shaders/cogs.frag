#version 330 core

out vec4 outColor;

uniform float time;
uniform vec2 offset;
uniform vec2 resolution;
uniform vec2 renderOffset;
uniform float blocksPerPixel;

in vec2 uv2;

const float pi = 3.14159;

float point(vec2 uv, float e, float w, float r) {
  return 1.0 - smoothstep(w, w + e, length(uv) - r);
}

float circle(vec2 uv, float e, float w, float r) {
  return 1.0 - smoothstep(w, w + e, abs(length(uv) - r));
}

float dottedCircle(vec2 uv, float e, float w, float r, float p) {
  float t = (atan(uv.y, uv.x) / pi) * 0.5 + 0.5;
  float s = floor(290.0 * 2.0 * pi * r);
  t = (floor(t * s) + 0.5) / s;
  t = (t * 2.0 - 1.0) * pi;
  return point(uv - vec2(cos(t), sin(t)) * r, e, w, p);
}

float dottedLine(vec2 uv, float e, float w, float p, vec2 a, vec2 b) {
  b -= a;
  uv -= a;
  vec2 l = normalize(b);
  uv = uv * mat2(l, vec2(-l.y, l.x));
  return point(vec2((fract(uv.x * 290.0) - 0.5) / 290.0, uv.y), e, w, p);
}

float rings(vec2 uv, float e, float w, float r, float s, float c) {
  float l = length(uv);
  return 1.0 -
         min(max(max(step(l, r), step(r + s * (c - 1.0), l)),
                 smoothstep(
                     w, w + e,
                     abs((fract((l - (s * 0.5)) / s - fract(r / s)) - 0.5) *
                         s))),
             smoothstep(w, w + e, abs(l - r)) *
                 smoothstep(w, w + e, abs(l - (r + (s * (c - 1.0))))));
}

float rings2(vec2 uv, float e, float w, float r, float s) {
  float l = length(uv);
  return 1.0 - smoothstep(w, w + e, abs(l - r)) *
                   smoothstep(w, w + e, abs(l - (r + s)));
}

mat2 rot(float t) {
  float s = sin(t);
  float c = cos(t);
  return mat2(c, s, -s, c);
}

float grid(vec2 fragCoord, float space, float gridWidth) {
  vec2 p = fragCoord - vec2(.5);
  vec2 size = vec2(gridWidth);

  vec2 a1 = mod(p - size, space);
  vec2 a2 = mod(p + size, space);
  vec2 a = a2 - a1;

  float g = min(a.x, a.y);
  return clamp(g, 0., 1.0);
}

float map(float value, float min1, float max1, float min2, float max2) {
  return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main() {
  float e = 1 / resolution.x;

  vec2 fragCoord = gl_FragCoord.xy - renderOffset + offset / 8.0;

  vec2 uv = (fragCoord - resolution.xy * 0.5) * e;
  uv.x = map(uv.x, 0, 1, 0, blocksPerPixel / 4.0);
  uv.y = map(uv.y, 0, 1, 0, blocksPerPixel / 4.0);

  e = 1.5 * e;

  const vec2 lineP = vec2(0.0, 0.19);
  float c = dottedLine(abs(uv), e, 0.0001, 0.0, lineP, lineP.yx);
  vec2 ruv = inversesqrt(2.0) * vec2(uv.x + uv.y, uv.x - uv.y);
  c += dottedLine(abs(ruv), e, 0.0002, 0.0, lineP, lineP.yx);

  c += dottedCircle(uv * rot(time * 0.4), e, 0.0, 0.01375, 0.0001);
  c += dottedCircle(uv * rot(-time * 0.3), e, 0.0, 0.03125, 0.0001);
  c += dottedCircle(uv * rot(time * 0.1), e, 0.0, 0.09625, 0.0001);
  c += dottedCircle(rot(time * 0.1) * uv, e, 0.0, 0.37, 0.0001);
  c *= 0.3;

  c += rings2(uv, e, 0.0, 0.008125, 0.0025);
  c += rings2(uv, e, 0.0, 0.02187, 0.05875);
  c += rings2(uv, e, 0.0, 0.10125, 0.06125);
  c += rings2(uv, e, 0.0, 0.439375, 0.03125);

  c += rings2(uv, e, 0.0005, 0.075, 0.040625);
  c += circle(uv, e, 0.0005, 0.339375);

  c += rings2(uv, e, 0.001, 0.026875, 0.163125);
  c += circle(uv, e, 0.001, 0.448125);

  vec2 p1 = rot(time * pi * 0.028) * uv - vec2(0.115625, 0.0);
  c += point(p1, e, 0.0, 0.004375);
  c += circle(p1, e, 0.0, 0.004375 * 2.0);

  vec2 p2 = rot(time * pi * 0.067 + 0.5) * uv - vec2(0.1625, 0.0);
  c += point(p2, e, 0.0, 0.0015625);
  c += rings2(p2, e, 0.0, 0.004375, 0.001875);

  vec2 p4 = rot(time * pi * 0.028 + pi * 1.2) * uv - vec2(0.339375, 0.0);
  c += point(p4, e, 0.0, 0.00875);
  c += rings2(p4, e, 0.0, 0.011875, 0.001875);
  c += circle(p4, e, 0.0, 0.021875);
  c += point(rot(time * pi * 0.143) * p4 - vec2(0.021875, 0.0), e, 0.0,
             0.003125);

  vec2 p5 = rot(time * pi * 0.00833 + pi * 1.3) * uv - vec2(0.448125, 0.0);
  c += point(p5, e, 0.0, 0.0028125);
  c += rings2(p5, e, 0.0, 0.00875, 0.003125);
  c += 0.3 * dottedCircle(rot(time) * p5, e, 0.0, 0.015, 0.0001);

  vec2 p6 = rot(time * pi * 0.011) * uv - vec2(0.439375 + 0.03125, 0.0);
  c += point(p6, e, 0.0, 0.005);
  c += 0.3 * dottedCircle(rot(time) * p6, e, 0.0, 0.0078125, 0.0001);
  c += circle(p6, e, 0.0, 0.0175);

  vec2 sp6 = rot(time * pi * 0.27) * p6 - vec2(0.0175, 0.0);
  c += point(sp6, e, 0.0, 0.0009375);
  c += circle(sp6, e, 0.0, 0.004375);

  outColor = vec4(vec3(0.3, 1.0, 0.8) * c, 1.0);

  // Grid.
  float regionGrid =
      grid(gl_FragCoord.xy + offset, 512.0 / blocksPerPixel, 0.75);
  if (regionGrid == 0) {
    outColor = vec4(1.0) * 0.25 + outColor * 0.75;
  }
}