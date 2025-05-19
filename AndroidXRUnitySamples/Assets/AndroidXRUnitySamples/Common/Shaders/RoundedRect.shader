// <copyright file="RoundedRect.shader" company="Google LLC">
//
// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
// ----------------------------------------------------------------------

Shader "AndroidXRUnitySamples/RoundedRect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Bounds ("Bounds", Vector) = (1, 1, 1, 1)
        _CornerRadius ("Corner Radius Scalar", Float) = 1
        _AAScalar ("Anti-alias Scalar", Float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            fixed4 _Bounds;
            float _CornerRadius;
            float _AAScalar;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color;
                return OUT;
            }

            float roundedBoxSDF(float2 pos, float2 rectSize, float cornerRadius)
            {
                float2 dist = abs(pos) - rectSize;
                return length(max(dist, 0.0)) + min(max(dist.x, dist.y), 0.0) - cornerRadius;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float smallerDim = min(_Bounds.x, _Bounds.y);
                float2 scaledUv = IN.texcoord - float2(0.5, 0.5);
                scaledUv *= _Bounds.xy;

                // Alpha is 0 on border, negative toward center, positive out.
                float borderAlpha = roundedBoxSDF(scaledUv, _Bounds.xy * 0.5 - _CornerRadius, _CornerRadius);

                float2 xGrad = float2(ddx(scaledUv.x), ddy(scaledUv.x));
                float2 yGrad = float2(ddx(scaledUv.y), ddy(scaledUv.y));
                float gradMagnitude = length(float2(length(xGrad), length(yGrad)));
                float antialiasDist = max(gradMagnitude * _AAScalar, 0.001);

                borderAlpha /= antialiasDist;

                // Scoot in 1 so the falloff ends at the edge of the mesh.
                borderAlpha += 1.0;
                borderAlpha = 1.0 - borderAlpha;

                half4 texColor = tex2D(_MainTex, IN.texcoord);
                fixed4 color = texColor * IN.color;
                color.a *= borderAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return color;
            }
        ENDCG
        }
    }
}

