// <copyright file="Outline.shader" company="Google LLC">
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

Shader "AndroidXRUnitySamples/Outline"
{
    Properties
    {
        _ExpandAmount ("Expand Amount", Float) = 0.001
        _ColorMapTex ("Color Map", 2D) = "black" {}
        _ColorSpeed ("Color Speed", Float) = 1
    }
    SubShader {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 300
        Cull Front
        Lighting Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            float _ExpandAmount;
            sampler1D _ColorMapTex;
            float _ColorSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                worldPos.xyz += worldNormal * _ExpandAmount;
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex1D(_ColorMapTex, _Time.y * _ColorSpeed);
            }
            ENDCG
        }
    }
}
