// <copyright file="AvatarHeadTracking.cs" company="Google LLC">
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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Tracks head data from the main camera and provides it as <see cref="AvatarHeadData"/>.
    /// </summary>
    public class AvatarHeadTracking : MonoBehaviour
    {
        [Tooltip("The avatar's reference frame.")]
        [SerializeField] private Transform _reference;

        [SerializeField, Tooltip("The input action to read the position value of a tracked " +
                                 "device. Must be a Vector 3 control type.")]
        InputActionProperty _positionInput;
        [SerializeField, Tooltip("The input action to read the rotation value of a tracked " +
                                 "device. Must be a Quaternion control type.")]
        InputActionProperty _rotationInput;

        /// <summary>The current head data.</summary>
        private AvatarHeadData _avatarHeadData = new AvatarHeadData();

        /// <summary>
        /// Gets the current head data.
        /// </summary>
        public AvatarHeadData HeadData => _avatarHeadData;

        /// <summary>
        /// Called before the first frame update.
        /// Deactivates in editor as head tracking requires a device.
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Class AvatarHeadTracking is designed for device runtime and will be " +
                      "deactivated in the editor.");
            enabled = false;
#endif
        }

        /// <summary>
        /// Called once per frame.
        /// Updates the head data based on the main camera's pose.
        /// </summary>
        private void Update()
        {
            Pose headPose = new Pose(_positionInput.action.ReadValue<Vector3>(),
                                     _rotationInput.action.ReadValue<Quaternion>());
            _avatarHeadData.LocalPose = headPose.InverseTransformedBy(_reference);
        }
    }
}
