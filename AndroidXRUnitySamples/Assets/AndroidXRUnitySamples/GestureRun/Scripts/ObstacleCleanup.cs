// <copyright file="ObstacleCleanup.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GestureRun
{
    /// <summary>
    /// Used to clean up physics objects which fall through the scene.
    /// </summary>
    public class ObstacleCleanup : MonoBehaviour
    {
        /// <summary>
        /// The Rigidbody component on the object. Finds one on the game object if not set.
        /// </summary>
        public Rigidbody Rigidbody;

        /// <summary>
        /// Cutoff threshold for when the object has been deemed to have dropped down enough
        /// and be destroyed.
        /// </summary>
        public float DestroyElevation = -20f;

        void Start()
        {
            if (Rigidbody == null)
            {
                Rigidbody = GetComponent<Rigidbody>();
            }
        }

        void Update()
        {
            if (transform.position.y <= DestroyElevation)
            {
                // Object has fallen down enough. Destroy it.
                Destroy(gameObject);
            }
        }
    }
}
