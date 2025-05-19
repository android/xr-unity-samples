// <copyright file="DecalMesh.cs" company="Google LLC">
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
using System.Collections.Generic;
using UnityEngine;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// A class which generates a decal mesh on the object.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class DecalMesh : MonoBehaviour
    {
        /// <summary>
        /// Number of seconds to wait until fade out starts.
        /// </summary>
        public float FadeoutStartTimeSeconds = 2.0f;

        /// <summary>
        /// Number of seconds it takes to smoothly fade out the decal.
        /// </summary>
        public float FadeoutDurationSeconds = 2.0f;

        private const string _kColorParam = "_BaseColor";
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private float _decalCreationTime = Mathf.Infinity;

        /// <summary>
        /// Create a decal on a provided mesh. The algorithm calculates vertex-box intersection
        /// rather than the slower triangle-box intersection. So at least one vertex must be in the
        /// decal range to be used.
        /// </summary>
        /// <param name="mesh">Original mesh to decal.</param>
        /// <param name="meshTransform">Transform of the gameobject containing the original mesh.</param>
        /// <param name="position">Position to place the decal.</param>
        /// <param name="orientation">Orientation to place the decal.</param>
        /// <param name="scale">Size of the decal.</param>
        public void CreateFromMesh(Mesh mesh, Matrix4x4 meshTransform, Vector3 position,
                                   Quaternion orientation, Vector3 scale)
        {
            Matrix4x4 projection = (Matrix4x4.Translate(position) * Matrix4x4.Rotate(orientation) *
                                    Matrix4x4.Scale(scale))
                                       .inverse;

            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            List<Vector2> uvs = new List<Vector2>(vertices.Count);
            bool[] vertexInBound = new bool[vertices.Count];
            Bounds unitBounds = new Bounds(Vector3.zero, Vector3.one);
            for (int i = 0; i < vertices.Count; i++)
            {
                // Apply the mesh transform to the vertices.
                Vector4 vertexPosition4d = vertices[i];
                vertexPosition4d.w = 1.0f;
                vertexPosition4d = meshTransform * vertexPosition4d;
                vertexPosition4d /= vertexPosition4d.w;
                vertices[i] = vertexPosition4d;

                // Compute the UVs.
                vertexPosition4d = projection * vertexPosition4d;
                vertexPosition4d /= vertexPosition4d.w;
                uvs.Add(new Vector2(vertexPosition4d.x + 0.5f, vertexPosition4d.y + 0.5f));
                vertexInBound[i] = unitBounds.Contains(vertexPosition4d);
            }

            // Prune triangles using vertexInBound.
            int numberOfUsedVertices = 0;
            int[] usedVertices = new int[vertices.Count];
            int[] originalTriangles = mesh.triangles;
            List<int> triangles = new List<int>(originalTriangles.Length);
            for (int i = 0; i + 2 < originalTriangles.Length; i += 3)
            {
                if (vertexInBound[originalTriangles[i]] ||
                    vertexInBound[originalTriangles[i + 1]] ||
                    vertexInBound[originalTriangles[i + 2]])
                {
                    triangles.Add(originalTriangles[i]);
                    triangles.Add(originalTriangles[i + 1]);
                    triangles.Add(originalTriangles[i + 2]);
                    numberOfUsedVertices += usedVertices[originalTriangles[i]];
                    usedVertices[originalTriangles[i]] = 1;
                    numberOfUsedVertices += usedVertices[originalTriangles[i + 1]];
                    usedVertices[originalTriangles[i + 1]] = 1;
                    numberOfUsedVertices += usedVertices[originalTriangles[i + 2]];
                    usedVertices[originalTriangles[i + 2]] = 1;
                }
            }

            // Prune vertices and get a mapping from original to new vertex indices.
            // Vertices and UVs will be stored in the original array to avoid allocations.
            // We will reused the usedVertices array for this mapping.
            int remappedVertices = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (usedVertices[i] > 0)
                {
                    vertices[remappedVertices] = vertices[i];
                    uvs[remappedVertices] = uvs[i];
                    usedVertices[i] = remappedVertices;
                    remappedVertices++;
                }
            }

            // Remap the triangles.
            for (int i = 0; i < triangles.Count; i++)
            {
                triangles[i] = usedVertices[triangles[i]];
            }

            vertices.RemoveRange(remappedVertices, vertices.Count - remappedVertices);
            uvs.RemoveRange(remappedVertices, uvs.Count - remappedVertices);
            Mesh decalMesh = new Mesh();
            decalMesh.SetVertices(vertices);
            decalMesh.SetUVs(0, uvs);
            decalMesh.SetTriangles(triangles, 0);
            decalMesh.RecalculateNormals();
            _meshFilter.sharedMesh = decalMesh;

            _decalCreationTime = Time.time;
        }

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            float currentTime = Time.time;
            float timeSinceDecalCreationSeconds = currentTime - _decalCreationTime;
            if (timeSinceDecalCreationSeconds > FadeoutStartTimeSeconds + FadeoutDurationSeconds)
            {
                // The decal has finished fading out.
                Destroy(gameObject);
                return;
            }
            else if (timeSinceDecalCreationSeconds > FadeoutStartTimeSeconds)
            {
                // Smoothly fade out the decal.
                float progress = (timeSinceDecalCreationSeconds - FadeoutStartTimeSeconds) /
                                 FadeoutDurationSeconds;
                progress = Mathf.SmoothStep(0.0f, 1.0f, progress);
                SetOpacity(1.0f - progress);
            }
        }

        private void SetOpacity(float opacity)
        {
            Color col = _meshRenderer.material.GetColor(_kColorParam);
            col.a = opacity;
            _meshRenderer.material.SetColor(_kColorParam, col);
        }
    }
}
