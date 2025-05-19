// <copyright file="ObjectMaterialRandomizer.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// Randomizes materials for object.
    /// </summary>
    public class ObjectMaterialRandomizer : MonoBehaviour
    {
        [SerializeField] private RendererMaterialCollection[] _matCollections;

        private void Start()
        {
            for (int i = 0; i < _matCollections.Length; ++i)
            {
                int randMat = UnityEngine.Random.Range(0, _matCollections[i].Materials.Length);
                for (int j = 0; j < _matCollections[i].Meshes.Length; ++j)
                {
                    _matCollections[i].Meshes[j].material = _matCollections[i].Materials[randMat];
                }
            }
        }
    }

    /// <summary>
    /// Helper class for ObjectMaterialRandomizer.
    /// </summary>
    [Serializable]
    public class RendererMaterialCollection
    {
        /// <summary>
        /// Rendererable mesh to swap materials on.
        /// </summary>
        public Renderer[] Meshes;

        /// <summary>
        /// List of materials to pick from.
        /// </summary>
        public Material[] Materials;
    }
}
