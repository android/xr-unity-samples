// <copyright file="DroneMeshDebugger.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Drone
{
    /// <summary>
    /// Show debug info for Drone when in debug mode.
    /// </summary>
    public class DroneMeshDebugger : MeshDebugger
    {
        [Tooltip("The material for AR meshes when it is not in the debug mode.")]
        [SerializeField] private Material _nonDebugMaterial;

        [Tooltip("The material for AR meshes when it is in the debug mode.")]
        [SerializeField] private Material _debugMaterial;

        /// <summary>
        /// Set up the mesh renderer according to the debug settings.
        /// </summary>
        /// <param name="meshRenderer">A mesh renderer guaranteed to be not null.</param>
        /// <param name="isDebugMode">Whether it is in the debug mode.</param>
        protected override void SetUpMeshRenderer(MeshRenderer meshRenderer, bool isDebugMode)
        {
            // Switch the material according to the debug settings.
            meshRenderer.sharedMaterial = isDebugMode ? _debugMaterial : _nonDebugMaterial;
        }
    }
}
