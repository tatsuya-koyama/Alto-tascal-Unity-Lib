#ifndef ALTO_SHADER_17_CUSTOM_EFFECT_CUBIC_COLOR
#define ALTO_SHADER_17_CUSTOM_EFFECT_CUBIC_COLOR

#include "../../Generic/AltoShaderUtil.hlsl"
#include "Constants.hlsl"

//------------------------------------------------------------------------------
// Cubic (6-directional) Color
//------------------------------------------------------------------------------

half3 CubicColor(float3 normalOS, float3 positionOS, float3 normalWS, float3 posWorld)
{
    half3 N = lerp(normalOS, normalWS, _WorldSpaceNormal);
    half dirTop    = max(0, dot(N, VecTop));
    half dirBottom = max(0, dot(N, VecBottom));
    half dirRight  = max(0, dot(N, VecRight));
    half dirFront  = max(0, dot(N, VecFront));
    half dirLeft   = max(0, dot(N, VecLeft));
    half dirBack   = max(0, dot(N, VecBack));

    half3 pos = lerp(positionOS, posWorld, _WorldSpaceGradient);

    float s, c;

    // Top
    sincos(_GradRotate_T, s, c);
    half rot_T = (pos.x - _GradOrigin_T.x) * -s
               + (pos.z - _GradOrigin_T.z) * c;
    half grad_T = saturate(rot_T / -_GradHeight_T);
    half3 color_T = lerp(_TopColor1, _TopColor2, grad_T);

    // Right
    sincos(_GradRotate_R, s, c);
    half rot_R = (pos.z - _GradOrigin_R.z) * -s
               + (pos.y - _GradOrigin_R.y) * c;
    half grad_R = saturate(rot_R / -_GradHeight_R);
    half3 color_R = lerp(_RightColor1, _RightColor2, grad_R);

    // Front
    sincos(_GradRotate_F, s, c);
    half rot_F = (pos.x - _GradOrigin_F.x) * -s
               + (pos.y - _GradOrigin_F.y) * c;
    half grad_F = saturate(rot_F / -_GradHeight_F);
    half3 color_F = lerp(_FrontColor1, _FrontColor2, grad_F);

    // Left
    sincos(_GradRotate_L, s, c);
    half rot_L = (pos.z - _GradOrigin_L.z) * s
               + (pos.y - _GradOrigin_L.y) * c;
    half grad_L = saturate(rot_L / -_GradHeight_L);
    half3 color_L = lerp(_LeftColor1, _LeftColor2, grad_L);

    // Back
    sincos(_GradRotate_B, s, c);
    half rot_B = (pos.x - _GradOrigin_B.x) * s
               + (pos.y - _GradOrigin_B.y) * c;
    half grad_B = saturate(rot_B / -_GradHeight_B);
    half3 color_B = lerp(_BackColor1, _BackColor2, grad_B);

    // Bottom
    sincos(_GradRotate_D, s, c);
    half rot_D = -(pos.x - _GradOrigin_D.x) * s
               + -(pos.z - _GradOrigin_D.z) * c;
    half grad_D = saturate(rot_D / -_GradHeight_D);
    half3 color_D = lerp(_BottomColor1, _BottomColor2, grad_D);

    half3 color;
    UNITY_BRANCH
    if (_MixCubicColorOn > 0)
    {
        half3 white = half3(1, 1, 1);
        color = lerp(color_T, white, 1 - dirTop) * lerp(color_D, white, 1 - dirBottom)
              * lerp(color_R, white, 1 - dirRight) * lerp(color_L, white, 1 - dirLeft)
              * lerp(color_F, white, 1 - dirFront) * lerp(color_B, white, 1 - dirBack);
    }
    else
    {
        color = (color_T * dirTop) + (color_D * dirBottom)
              + (color_R * dirRight) + (color_L * dirLeft)
              + (color_F * dirFront) + (color_B * dirBack);
    }
    return color;
}

#endif
