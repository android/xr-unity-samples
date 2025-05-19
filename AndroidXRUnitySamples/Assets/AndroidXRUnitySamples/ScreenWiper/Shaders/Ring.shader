// <copyright file="Ring.shader" company="Google LLC">
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

Shader "AndroidXRUnitySamples/Ring"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Color("Color", Color) = (1,1,1,1)
        _OuterRad("Outer Radius", Float) = 1
        _InnerRad("Inner Radius", Float) = 1
    }

    SubShader{
        Tags{ "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

        LOD 100
        Cull Back
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _OuterRad;
            fixed _InnerRad;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 toCenter = i.uv;
                toCenter -= float2(0.5, 0.5);
                float distToCenter = length(toCenter);
                distToCenter *= 2;

                // Discard beyond the outer radius.
                clip(_OuterRad - distToCenter);

                // Discard below the inner radius.
                clip(distToCenter - _InnerRad);

                return _Color;
            }
            ENDCG
        }
    }
}
