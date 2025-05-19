// <copyright file="DepthMesh.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Class used to update a mesh based on the Depth Texture.
    /// </summary>
    public class DepthMesh : MonoBehaviour
    {
        [SerializeField]
        private DepthFrameVariable _depthFrame;

        [Space]
        [SerializeField]
        private float _minValidDepth = 0.01f;
        [SerializeField]
        private float _invalidDepthReplacement = 5f;

        [Space]
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshCollider _meshCollider;
        [SerializeField] private float _meshColliderDownsampleFactor = 4.0f;

        private NativeArray<Vector3> _vertices;
        private Mesh _mesh;
        private NativeArray<Vector3> _colliderVertices;
        private Mesh _colliderMesh;

        private static void AllocateDepthMesh(int width, int height,
                                              ref NativeArray<Vector3> vertices, ref Mesh mesh)
        {
            int vertexCount = width * height;
            vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);

            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                vertices = new Vector3[vertexCount]
            };
            mesh.MarkDynamic();

            // Populate triangles with a regular grid.
            int[] triangles = new int[(width - 1) * (height - 1) * 6];

            int index = 0;
            for (int v = 0; v < width - 1; v++)
            {
                for (int u = 0; u < height - 1; u++)
                {
                    int i0 = v * width + u;
                    int i1 = i0 + 1;
                    int i2 = i0 + width;
                    int i3 = i2 + 1;

                    triangles[index++] = i0;
                    triangles[index++] = i2;
                    triangles[index++] = i1;

                    triangles[index++] = i1;
                    triangles[index++] = i2;
                    triangles[index++] = i3;
                }
            }

            mesh.triangles = triangles;
        }

        private void OnEnable()
        {
            _depthFrame.AddListener(OnDepthFrameReceived);
        }

        private void OnDisable()
        {
            _depthFrame.RemoveListener(OnDepthFrameReceived);
        }

        private void OnDestroy()
        {
            _vertices.Dispose();
        }

        private void OnDepthFrameReceived(DepthFrame depthFrame)
        {
            int downsampledWidth =
                (int)Mathf.Round(depthFrame.Texture.width / _meshColliderDownsampleFactor);
            int downsampledHeight =
                (int)Mathf.Round(depthFrame.Texture.height / _meshColliderDownsampleFactor);
            if (_mesh == null)
            {
                AllocateDepthMesh(depthFrame.Texture.width, depthFrame.Texture.height,
                                  ref _vertices, ref _mesh);
                _meshFilter.sharedMesh = _mesh;
                AllocateDepthMesh(downsampledWidth, downsampledHeight, ref _colliderVertices,
                                  ref _colliderMesh);
            }

            (float fx, float fy, float cx, float cy) = depthFrame.GetIntrinsics();

            NativeArray<float> pixels = depthFrame.Texture.GetPixelData<float>(0);
            {
                // Full resolution depth mesh.
                var depthMeshJob = new DepthMeshJob()
                {
                    DepthPixels = pixels,
                    SrcWidth = depthFrame.Texture.width,
                    SrcHeight = depthFrame.Texture.height,
                    DstWidth = depthFrame.Texture.width,
                    DstHeight = depthFrame.Texture.height,
                    MinValidDepth = _minValidDepth,
                    InvalidDepthReplacement = _invalidDepthReplacement,
                    Focal = new Vector2(fx, fy),
                    Center = new Vector2(cx, cy),
                    Vertices = _vertices,
                };

                JobHandle jobHandle = depthMeshJob.Schedule(
                    _vertices.Length, depthFrame.Texture.width);
                jobHandle.Complete();

                _mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Length, 0,
                                        MeshUpdateFlags.DontValidateIndices);
                _mesh.RecalculateBounds();
                _mesh.MarkModified();
            }

            {
                // Compute a downsampled depth mesh for the collider.
                var colliderDepthMeshJob = new DepthMeshJob()
                {
                    DepthPixels = pixels,
                    SrcWidth = depthFrame.Texture.width,
                    SrcHeight = depthFrame.Texture.height,
                    DstWidth = downsampledWidth,
                    DstHeight = downsampledHeight,
                    MinValidDepth = _minValidDepth,
                    InvalidDepthReplacement = _invalidDepthReplacement,
                    Focal = new Vector2(fx, fy) / _meshColliderDownsampleFactor,
                    Center = new Vector2(cx, cy) / _meshColliderDownsampleFactor,
                    Vertices = _colliderVertices,
                };

                JobHandle colliderDepthMeshJobHandle =
                    colliderDepthMeshJob.Schedule(_colliderVertices.Length, downsampledWidth);
                colliderDepthMeshJobHandle.Complete();
                _colliderMesh.SetVertexBufferData(
                    _colliderVertices, 0, 0, _colliderVertices.Length, 0,
                    MeshUpdateFlags.DontValidateIndices);
                _colliderMesh.RecalculateBounds();
                _colliderMesh.MarkModified();
                _meshCollider.sharedMesh = _colliderMesh;
            }
        }
    }
}
