// <copyright file="WaterSurface.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using UnityEngine;

    /// <summary>
    /// Class which manages the water surface, increasing it's level during startup.
    /// </summary>
    public class WaterSurface : MonoBehaviour
    {
        private static readonly int _waterSurfacePointProperty =
            Shader.PropertyToID("_WaterSurfacePoint");

        [SerializeField] private Material _underwaterFullscreenMaterial;
        [SerializeField] private float _maxHeight;
        [SerializeField] private float _waterRisingSpeed;

        private void Update()
        {
            transform.position = new Vector3(transform.position.x,
                Mathf.Min(transform.position.y + _waterRisingSpeed * Time.deltaTime, _maxHeight),
                transform.position.z);
            _underwaterFullscreenMaterial.SetVector(_waterSurfacePointProperty, transform.position);
        }
    }
}
