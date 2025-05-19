// <copyright file="LookAtCamera.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// The LookAtCamera script.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private bool _allowPitch;

        void UpdateCameraRotation()
        {
            // Calculate the direction vector from this object to the main camera.
            Vector3 directionToCamera = transform.position -
                Singleton.Instance.Camera.transform.position;

            if (!_allowPitch)
            {
                directionToCamera.y = 0.0f;
                directionToCamera.Normalize();
            }

            // Set the object's rotation to the desired rotation.
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }

        void Update()
        {
            UpdateCameraRotation();
        }
    }
}
