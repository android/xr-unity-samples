// <copyright file="ShadowShader.shader" company="Google LLC">
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

// The shader provides the following features:
// 1. Make the object totally transparent.
// 2. Receive soft shadow (with partially transparent edges).
// 3. Occlude opaque objects behind (at pixels where other objects have larger z values).
// 4. Provide an input argument to shrink the object (the _InwardOffset input) before applying
//    occlusion and receiving shadow.
Shader "AndroidXRUnitySamples/SimpleShadows"
{
    Properties
    {
        // Shrink the surface inward by an offset along the negative vertex normal directions.
        _InwardOffset ("Inward Offset (meters)", Float) = 0
    }

    // Based on https://docs.unity3d.com/6000.0/Documentation/Manual/urp/use-built-in-shader-methods-shadows.html
    SubShader
    {

        Tags { "Queue"="Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Back   // Only render the front-facing faces.
        Blend SrcAlpha Zero // Discard background color to achieve occlusion effect.

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            // See https://discussions.unity.com/t/unitys-urp-shadow-shader-example-gives-sharp-shadows-only/1546483/3
            #pragma multi_compile_fragment _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _InwardOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS  : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Calculate the offset vertex position by moving it along the negative vertex
                // normal direction in the object space by an amount specified by an input argument.
                float3 offsetPositionOS = IN.positionOS.xyz - _InwardOffset * IN.normalOS.xyz;

                OUT.positionCS = TransformObjectToHClip(offsetPositionOS);
                OUT.positionWS = TransformObjectToWorld(offsetPositionOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 shadowCoords = TransformWorldToShadowCoord(IN.positionWS);
                float shadowAmount = MainLightRealtimeShadow(shadowCoords);

                // Set the fragment color to the shadow value
                return half4(0.0, 0.0, 0.0, 1.0 - shadowAmount);
            }
            ENDHLSL
        }
    }
}
