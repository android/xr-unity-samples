// <copyright file="ScreenWiper.shader" company="Google LLC">
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

Shader "AndroidXRUnitySamples/ScreenWiper"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _ColorMapTex ("Color Map", 2D) = "black" {}
        _MoveSpeed ("Move Speed", Float) = 1
        _ColorSpeed ("Color Speed", Float) = 1
        _HoleColor ("Hole Color", Color) = (0, 0, 0, 1)
        _PulseSpeed ("Pulse Speed", Float) = 1
        _PulseAmount ("Pulse Amount", Float) = 0.025
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        LOD 100
        Cull Back
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            sampler1D _ColorMapTex;
            float _MoveSpeed;
            float _ColorSpeed;
            float4 _HoleColor;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvMask = TRANSFORM_TEX(v.uv, _MaskTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample at uv scale 1.
                float2 uv1 = i.uv;
                uv1.x += sin(_Time.x * 4.89 * _MoveSpeed);
                uv1.y += _Time.y * .123 * _MoveSpeed;
                fixed4 tex1 = tex2D(_MainTex, uv1);

                // Sample at uv scale 2.
                float2 uv2 = i.uv * 2;
                uv2.x += _Time.y * 0.277 * _MoveSpeed;
                uv2.y += sin(_Time.x * 6.231 * _MoveSpeed);
                fixed4 tex2 = tex2D(_MainTex, uv2);

                float totalValue = (tex1.r * 0.75) + (tex2.r * 0.25);
                fixed4 mapColor = tex1D(_ColorMapTex, totalValue + _Time.x * _ColorSpeed);
                float4 col = saturate(mapColor);
                col.a = 1;

                // Mask.
                float pulseInside = sin(_Time.y * _PulseSpeed + i.uv.x * 150) * _PulseAmount;
                float pulseOutside = cos(_Time.y * _PulseSpeed + i.uv.x * 150) * _PulseAmount;
                fixed4 mask = tex2D(_MaskTex, i.uvMask);
                float4 tintedCol = lerp(col, _HoleColor, 0.5);
                col = lerp(tintedCol, col, step(0.8 + pulseOutside, mask.r));
                col.a = lerp(0, max(col.a, tintedCol.a), step(0.5 + pulseInside, mask.r));
                return col;
            }
            ENDCG
        }
    }
}
