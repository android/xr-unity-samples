// <copyright file="DepthMeshJob.cs" company="Google LLC">
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

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Job used for computing depth mesh vertex positions in parallel.
    /// </summary>
    [BurstCompile]
    public struct DepthMeshJob : IJobParallelFor
    {
        /// <summary>Depth pixels from the Depth Texture.</summary>
        [ReadOnly]
        public NativeArray<float> DepthPixels;

        /// <summary>Width of the Depth Texture.</summary>
        [ReadOnly]
        public int SrcWidth;

        /// <summary>Height of the Depth Texture.</summary>
        [ReadOnly]
        public int SrcHeight;

        /// <summary>Width of the Depth Mesh.</summary>
        [ReadOnly]
        public int DstWidth;

        /// <summary>Height of the Depth Mesh.</summary>
        [ReadOnly]
        public int DstHeight;

        /// <summary>Focal intrinsics of the depth view.</summary>
        [ReadOnly]
        public Vector2 Focal;

        /// <summary>Center intrinsics of the depth view.</summary>
        [ReadOnly]
        public Vector2 Center;

        /// <summary>
        /// Minimum valid depth value. Any values less than this will be replaced with `InvalidDepthReplacement`.
        /// </summary>
        [ReadOnly]
        public float MinValidDepth;

        /// <summary>Value to use to replace invalid depth values with.</summary>
        [ReadOnly]
        public float InvalidDepthReplacement;

        /// <summary>Output vertex positions.</summary>
        public NativeArray<Vector3> Vertices;

        /// <summary>Method used by Unity for executing a single iteration of a job.</summary>
        /// <param name="index">Pixel index in the Depth Texture.</param>
        public void Execute(int index)
        {
            int v = index / DstWidth;
            int u = index % DstWidth;

            // Flip texture along y-axis.
            int src_v = (int)Mathf.Round((float)v * (SrcHeight - 1) / (DstHeight - 1));
            int src_u = (int)Mathf.Round((float)u * (SrcWidth - 1) / (DstWidth - 1));
            int srcIndex = src_v * SrcWidth + (SrcHeight - 1 - src_u);

            float depth = DepthPixels[srcIndex];
            if (depth < MinValidDepth)
            {
                depth = InvalidDepthReplacement;
            }

            var p = new Vector3(
                (u - Center.x) * depth / Focal.x,
                (v - Center.y) * depth / Focal.y,
                depth);
            Vertices[index] = p;
        }
    }
}
