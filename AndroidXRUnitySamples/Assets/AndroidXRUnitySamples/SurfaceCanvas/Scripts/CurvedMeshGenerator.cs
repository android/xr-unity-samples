// <copyright file="CurvedMeshGenerator.cs" company="Google LLC">
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
using UnityEngine;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Generates a parameterized curved rectangular mesh.
    /// </summary>
    public static class CurvedMeshGenerator
    {
        /// <summary>
        /// Generates a curved rectangular mesh from the given parameters.
        /// </summary>
        /// <param name="resolution">Vertex density of the mesh.</param>
        /// <param name="radius">Radius of the curve.</param>
        /// <param name="height">Height of the mesh.</param>
        /// <param name="arcAngleDegrees">Arc angle covered by the mesh.</param>
        /// <returns>A Mesh object representing a rectangular curved mesh.</returns>
        public static Mesh GenerateCurvedMesh(int resolution, float radius, float height,
                                              float arcAngleDegrees)
        {
            var mesh = new Mesh();
            var vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[6 * (resolution - 1) * (resolution - 1)];
            int triangleIndex = 0;
            Vector2[] uvs = mesh.uv.Length == vertices.Length
                                    ? mesh.uv
                                    : new Vector2[vertices.Length];
            float angleStep = (float)(Math.PI / 180f * arcAngleDegrees) / resolution;
            float heightStep = height / resolution;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = x + y * resolution;
                    float angle = x * angleStep;

                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    vertices[i] = new Vector3(radius * Mathf.Cos(angle), y * heightStep,
                            radius * Mathf.Sin(angle));
                    uvs[i] = new Vector2(1 - percent.x, percent.y);

                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triangleIndex++] = i;
                        triangles[triangleIndex++] = i + resolution + 1;
                        triangles[triangleIndex++] = i + resolution;
                        triangles[triangleIndex++] = i;
                        triangles[triangleIndex++] = i + 1;
                        triangles[triangleIndex++] = i + resolution + 1;
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.uv = uvs;
            return mesh;
        }
    }
}
