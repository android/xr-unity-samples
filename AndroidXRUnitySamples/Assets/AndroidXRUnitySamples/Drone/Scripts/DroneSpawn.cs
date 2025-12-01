// <copyright file="DroneSpawn.cs" company="Google LLC">
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
    /// Handles visuals for drone spawning placement.
    /// </summary>
    public class DroneSpawn : MonoBehaviour
    {
        [SerializeField] private LayerMask _sceneLayer;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        private BoxCollider _collider;
        private Renderer[] _meshes;
        private bool _wasCollidingWithScene;

        private void Start()
        {
            _collider = GetComponent<BoxCollider>();
            _meshes = GetComponentsInChildren<Renderer>();
            _wasCollidingWithScene = false;
            UpdateMeshColors();
        }

        private void Update()
        {
            Vector3 worldCenter = transform.TransformPoint(_collider.center);
            Vector3 halfExtents = Vector3.Scale(_collider.size, transform.lossyScale) * 0.5f;
            Quaternion orientation = transform.rotation;

            bool collidingWithScene = Physics.CheckBox(worldCenter, halfExtents, orientation,
                _sceneLayer, QueryTriggerInteraction.Ignore);
            if (_wasCollidingWithScene != collidingWithScene)
            {
                _wasCollidingWithScene = collidingWithScene;
                UpdateMeshColors();
            }
        }

        private void UpdateMeshColors()
        {
            for (int i = 0; i < _meshes.Length; ++i)
            {
                _meshes[i].material.color = _wasCollidingWithScene ? _invalidColor : _validColor;
            }
        }
    }
}
