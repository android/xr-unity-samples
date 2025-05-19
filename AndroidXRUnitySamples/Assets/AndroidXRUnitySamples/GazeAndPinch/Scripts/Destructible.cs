// <copyright file="Destructible.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used to represent a destructible object.
    /// </summary>
    public class Destructible : MonoBehaviour
    {
        /// <summary>
        /// Settings used to get information from.
        /// </summary>
        public DestructibleSettings Settings;

        /// <summary>
        /// The MeshRenderer of the object.
        /// </summary>
        public MeshRenderer MeshRenderer;

        /// <summary>
        /// The dust particles prefab to use.
        /// </summary>
        public GameObject DustPrefab;

        /// <summary>
        /// The local position offsets to use to spawn new dust particle prefabs.
        /// </summary>
        public Vector3[] DustSpawnLocations;

        float _health;

        void Start()
        {
            _health = Settings.Health;
            MeshRenderer.material = Settings.FullHealthMaterial;
        }

        void OnCollisionEnter(Collision collision)
        {
            float force = collision.impulse.magnitude / Time.fixedDeltaTime;

            _health -= force;

            if (Settings.IsSeverelyDamaged(_health))
            {
                MeshRenderer.material = Settings.SeverelyDamagedHealthMaterial;
            }
            else if (Settings.IsDamaged(_health))
            {
                MeshRenderer.material = Settings.DamagedHealthMaterial;
            }
            else
            {
                MeshRenderer.material = Settings.FullHealthMaterial;
            }

            if (_health <= 0)
            {
                HandleDeath();
                return;
            }
        }

        void HandleDeath()
        {
            foreach (Vector3 spawnLocation in DustSpawnLocations)
            {
                GameObject g = Instantiate(DustPrefab);
                g.transform.position = transform.position + spawnLocation;
            }

            Destroy(gameObject);
        }
    }
}
