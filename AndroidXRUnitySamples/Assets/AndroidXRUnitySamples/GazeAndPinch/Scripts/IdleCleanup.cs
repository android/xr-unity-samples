// <copyright file="IdleCleanup.cs" company="Google LLC">
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
    /// Used to clean up physics objects which are no longer moving in the scene.
    /// </summary>
    public class IdleCleanup : MonoBehaviour
    {
        /// <summary>
        /// The Rigidbody component on the object.
        /// </summary>
        public Rigidbody Rigidbody;

        /// <summary>
        /// Cutoff threshold for when the object has been deemed to have dropped down enough
        /// and be destroyed.
        /// </summary>
        public float DestroyElevation = -20f;

        /// <summary>
        /// Duration (in seconds) of how long the object can be idle (i.e., zero velocity)
        /// before being destroyed. Timer starts only after Activate() has been called.
        /// </summary>
        public float IdleTime = 3f;

        float _destroyTimer = 0f;
        bool _checkForIdleState = false;

        /// <summary>
        /// Start the timer to ensure the object is cleaned up after being idle long enough.
        /// </summary>
        public void Activate()
        {
            _checkForIdleState = true;
            _destroyTimer = IdleTime;
        }

        void Update()
        {
            if (transform.position.y <= DestroyElevation)
            {
                // Object has fallen down enough. Destroy it.
                Destroy(gameObject);
            }

            if (_checkForIdleState)
            {
                if (Mathf.Approximately(Rigidbody.linearVelocity.sqrMagnitude, 0))
                {
                    _destroyTimer -= Time.deltaTime;
                    if (_destroyTimer <= 0)
                    {
                        // Object has been static enough. Time to destroy it.
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    // Object is still moving. Reset the clock.
                    _destroyTimer = IdleTime;
                }
            }
        }
    }
}
