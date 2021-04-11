#ifndef ALTO_SHADER_UTIL
#define ALTO_SHADER_UTIL

//==============================================================================
// Random
//==============================================================================

float rand(float2 seed)
{
    return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
}

float rand(float3 seed)
{
    return frac(sin(dot(seed.xyz, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

//==============================================================================
// Mapping
//==============================================================================

float map(float value, float min1, float max1, float min2, float max2)
{
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

float2 map(float2 value, float min1, float max1, float min2, float max2)
{
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

float3 map(float3 value, float min1, float max1, float min2, float max2)
{
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

float posterize(float val, int numStep)
{
    float mapped = map(val, 0, 1, 0, numStep);
    return floor(mapped) / (numStep - 1.0);
}

float3 posterize(float3 val, int numStep)
{
    float3 mapped = map(val, 0, 1, 0, numStep);
    return floor(mapped) / (numStep - 1.0);
}

//==============================================================================
// Noise
//==============================================================================

float noise(float3 pos)
{
    float3 ip = floor(pos);
    float3 fp = smoothstep(0, 1, frac(pos));
    float4 a = float4(
        rand(ip + float3(0, 0, 0)),
        rand(ip + float3(1, 0, 0)),
        rand(ip + float3(0, 1, 0)),
        rand(ip + float3(1, 1, 0))
    );
    float4 b = float4(
        rand(ip + float3(0, 0, 1)),
        rand(ip + float3(1, 0, 1)),
        rand(ip + float3(0, 1, 1)),
        rand(ip + float3(1, 1, 1))
    );
    a = lerp(a, b, fp.z);
    a.xy = lerp(a.xy, a.zw, fp.y);
    return lerp(a.x, a.y, fp.x);
}

float perlin(float3 pos) {
    return (
        noise(pos) * 32 +
        noise(pos * 2 ) * 16 +
        noise(pos * 4) * 8 +
        noise(pos * 8) * 4 +
        noise(pos * 16) * 2 +
        noise(pos * 32)
    ) / 63;
}

//==============================================================================
// Color
//==============================================================================

half3 shiftColor(half3 rgb, half3 hsv)
{
    half3 color = half3(rgb);
    float VSU = hsv.z * hsv.y * cos(hsv.x * 3.14159265 / 180);
    float VSW = hsv.z * hsv.y * sin(hsv.x * 3.14159265 / 180);

    color.x = (.299 * hsv.z + .701 * VSU + .168 * VSW) * rgb.x
            + (.587 * hsv.z - .587 * VSU + .330 * VSW) * rgb.y
            + (.114 * hsv.z - .114 * VSU - .497 * VSW) * rgb.z;

    color.y = (.299 * hsv.z - .299 * VSU - .328 * VSW) * rgb.x
            + (.587 * hsv.z + .413 * VSU + .035 * VSW) * rgb.y
            + (.114 * hsv.z - .114 * VSU + .292 * VSW) * rgb.z;

    color.z = (.299 * hsv.z - .300 * VSU + 1.25 * VSW) * rgb.x
            + (.587 * hsv.z - .588 * VSU - 1.05 * VSW) * rgb.y
            + (.114 * hsv.z + .886 * VSU - .203 * VSW) * rgb.z;

    return color;
}

//==============================================================================
// Rotate
//==============================================================================

half2 rotate(half2 v, float angle) {
    float s, c;
    sincos(angle, s, c);
    half2x2 m = half2x2(c, -s, s, c);
    return mul(m, v);
}

#endif
