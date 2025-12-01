// <copyright file="DepthPointCloud.shader" company="Google LLC">
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
Shader "SeaThrough/DepthPointCloud"
{
    SubShader
    {
        Tags {"Queue"="Background" "IgnoreProjector"="True"}
        Cull Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                uint id : SV_VertexID;
            };

            struct v2f
            {
                float4 color : COLOR;
                float size: PSIZE;
                float4 viewPosition: POSITION1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _DepthRes;
            float4 _TanFov;
            float4x4 _DepthCamera;
            float4x4 _ViewProjectionMatrix;
            float4x4 _ViewMatrix;

            v2f vert (appdata v)
            {
                v2f o;

                float2 uv = float2(
                        ((v.id % (int)_DepthRes) + 0.5) / _DepthRes,
                        ((v.id / (int)_DepthRes) + 0.5) / _DepthRes);

                float depth = tex2Dlod(_MainTex, float4(uv, 0.0, 0.0)).x;

                if (depth < 0.2)
                {
                    o.size = 0;
                    o.vertex = 0;
                    o.color = 0;
                    return o;
                }

                // The depth camera's near plane at z=1 is parameterized by
                // z = 1
                // x = lerp(tanL, tanR, u)
                // y = lerp(tanB, tanT, v)
                float3 near = float3(lerp(_TanFov.xz, _TanFov.yw, uv), 1.0);

                float3 cameraPos = near*depth;

                float4 worldPos = mul(_DepthCamera, float4(cameraPos, 1.0));

                o.size = 3.0;

                o.vertex = mul(_ViewProjectionMatrix, worldPos);
                o.viewPosition = mul(_ViewMatrix, worldPos);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 viewPosition = i.viewPosition.xyz / i.viewPosition.w;
                return float4(-viewPosition.z, -viewPosition.z, -viewPosition.z, 1.0);
            }
            ENDCG
        }
    }
}

