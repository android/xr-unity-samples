// <copyright file="CurvedMesh.cs" company="Google LLC">
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

using UnityEngine;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// A 3D rectangular curved mesh that can be added to the scene.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class CurvedMesh : MonoBehaviour
    {
        [Range(2, 256)]
        [SerializeField]
        private int _resolution = 10;

        [SerializeField]
        private float _radius = 20f;

        [SerializeField]
        private float _height = 30f;

        [Range(0f, 360f)]
        [SerializeField]
        private float _arcAngleDegrees = 180f;

        [SerializeField]
        private Material _curvedMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private bool _dirty;

        private void OnValidate()
        {
            _dirty = true;
        }

        private void Update()
        {
            if (_dirty)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }

            _meshRenderer.sharedMaterial = _curvedMaterial;

            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            _meshFilter.sharedMesh =
                    CurvedMeshGenerator.GenerateCurvedMesh(_resolution, _radius, _height,
                            _arcAngleDegrees);

            if (_meshCollider == null)
            {
                _meshCollider = GetComponent<MeshCollider>();
            }

            _meshCollider.sharedMesh = _meshFilter.sharedMesh;

            _dirty = false;
        }
    }
}
