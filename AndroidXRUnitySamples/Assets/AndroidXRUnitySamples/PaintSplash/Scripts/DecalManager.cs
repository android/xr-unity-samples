// <copyright file="DecalManager.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// A class which generates and holds decal objects on the provided depth mesh.
    /// </summary>
    public class DecalManager : MonoBehaviour
    {
        private const string _kColorParam = "_BaseColor";
        [SerializeField]
        private GameObject _depthMesh;
        [SerializeField]
        private GameObject _decalPrefab;
        [SerializeField]
        private float _decalScale = 1.0f;

        private MeshFilter _depthMeshMeshFilter;

        /// <summary>
        /// Generates a decal the specified position and orientation onto the depth mesh.
        /// </summary>
        /// <param name="position">Position to place the decal.</param>
        /// <param name="orientation">Orientation to place the decal.</param>
        /// <param name="color">Color of the decal.</param>
        public void GenerateDecal(Vector3 position, Quaternion orientation, Color color)
        {
            // Instantiate the decal prefab
            GameObject decal = Instantiate(_decalPrefab, transform);
            if (decal.TryGetComponent<DecalMesh>(out var decalMesh))
            {
                decalMesh.CreateFromMesh(_depthMeshMeshFilter.sharedMesh,
                                         _depthMesh.transform.localToWorldMatrix, position,
                                         orientation, _decalScale * Vector3.one);
                if (decalMesh.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    meshRenderer.material.SetColor(_kColorParam, color);
                }
            }
        }

        void Start()
        {
            _depthMeshMeshFilter = _depthMesh.GetComponent<MeshFilter>();
        }
    }
}
