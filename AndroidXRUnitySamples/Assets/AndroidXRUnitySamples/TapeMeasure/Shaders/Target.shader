// <copyright file="Target.shader" company="Google LLC">
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

Shader "AndroidXRUnitySamples/Target"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _OuterColor("Outer Color", Color) = (1,1,1,1)
        _InnerColor("Inner Color", Color) = (1,1,1,1)
        _OuterRad("Outer Radius", Float) = 1
        _InnerRad("Inner Radius", Float) = 1
    }

    SubShader{
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            fixed4 _OuterColor;
            fixed4 _InnerColor;
            fixed _OuterRad;
            fixed _InnerRad;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float inverseLerp(float a, float b, float value)
            {
                return (value - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Distance from center of quad.
                float dist = distance(i.uv, float2(0.5f, 0.5f));

                // Get the rate of change of dist on x and y.
                float2 distGrad = float2(ddx(dist), ddy(dist));
                float gradMagnitude = length(distGrad);
                float antialiasDist = max(gradMagnitude, 0.001f);

                // Bring _OuterRad in a few pixels so we have room for a gradient outward to
                // anti-alias the circle.
                // These "few pixels" are determined by our derivative calculation above.
                float outerRadius = _OuterRad - antialiasDist;
                float deltaToOuter = dist - outerRadius;
                float clampedOuterDelta = clamp(deltaToOuter, 0.0f, antialiasDist);
                float outerAlpha = 1.0f - (clampedOuterDelta / antialiasDist);

                float innerRadius = _InnerRad - antialiasDist;
                float deltaToInner = dist - innerRadius;
                float clampedInnerDelta = clamp(deltaToInner, 0.0f, antialiasDist);
                float innerAlpha = (clampedInnerDelta / antialiasDist);

                float gradientT = inverseLerp(_OuterRad, _InnerRad, dist);
                fixed4 col = lerp(_OuterColor, _InnerColor, gradientT);
                col.a = outerAlpha * innerAlpha;
                return col;
            }
            ENDCG
        }
    }
}
