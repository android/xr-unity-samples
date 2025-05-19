// <copyright file="MeshFilterControls.cs" company="Google LLC">
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
    /// Exposes some properties of an assigned MeshFilter for controlling via UI events and scripts.
    /// </summary>
    public class MeshFilterControls : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter _meshFilter;

        [SerializeField]
        private Mesh[] _meshes;

        /// <summary>
        /// Set the MeshFilter's mesh.
        /// </summary>
        /// <param name="mesh">The mesh to set.</param>
        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
            if (_meshFilter.gameObject.TryGetComponent(out MeshCollider mc))
            {
                mc.sharedMesh = mesh;
            }
        }

        /// <summary>
        /// Set the MeshFilter's mesh.
        /// </summary>
        /// <param name="index">The index of the mesh to set.</param>
        public void SetMesh(int index)
        {
            SetMesh(_meshes[index % _meshes.Length]);
        }

        /// <summary>
        /// Set mesh to Cube mesh from a ToggleGroup in the UI.
        /// </summary>
        /// <param name="isOn">Whether to toggle the cube mesh on.</param>
        public void SetMeshCubeToggle(bool isOn)
        {
            if (isOn)
            {
                SetMesh(_meshes[0]);
            }
        }

        /// <summary>
        /// Set mesh to Cone mesh from a ToggleGroup in the UI.
        /// </summary>
        /// <param name="isOn">Whether to toggle to Cone mesh on.</param>
        public void SetMeshConeToggle(bool isOn)
        {
            if (isOn)
            {
                SetMesh(_meshes[1]);
            }
        }

        /// <summary>
        /// Set mesh to Cylinder mesh from a ToggleGroup in the UI.
        /// </summary>
        /// <param name="isOn">Whether to toggle to Cylinder mesh on.</param>
        public void SetMeshCylinderToggle(bool isOn)
        {
            if (isOn)
            {
                SetMesh(_meshes[2]);
            }
        }

        /// <summary>
        /// Resets all parameters in the referenced MeshFilter to their starting values.
        /// </summary>
        public void Reset()
        {
            SetMeshCubeToggle(true);
        }
    }
}
