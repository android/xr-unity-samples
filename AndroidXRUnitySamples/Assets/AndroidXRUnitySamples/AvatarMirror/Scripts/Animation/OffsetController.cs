// <copyright file="OffsetController.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Controls and applies a rotational offset based on configurable axis rotations.
    /// This can be used for fine-tuning avatar rotations or other spatial adjustments.
    /// </summary>
    public class OffsetController : MonoBehaviour
    {
        /// <summary>
        /// The calculated rotational offset as a Quaternion. This can be used by other scripts.
        /// </summary>
        internal Quaternion _rotationOffset = Quaternion.identity;

        [Tooltip("The factor by which input rotation values are multiplied to get the final " +
                 "angle in degrees.")]
        [SerializeField] private int _angleFactor = 15;

        [Space]

        [Tooltip("The current rotation around the X-axis in degrees.")]
        [SerializeField] private int _xRotation = 0;

        [Tooltip("The current rotation around the Y-axis in degrees.")]
        [SerializeField] private int _yRotation = 0;

        [Tooltip("The current rotation around the Z-axis in degrees.")]
        [SerializeField] private int _zRotation = 0;

        /// <summary>
        /// Updates the X-axis rotation and recalculates the total rotation offset.
        /// </summary>
        /// <param name="rotation">The input value to be multiplied by <see cref="_angleFactor"/>
        /// for X-axis rotation.</param>
        public void UpdateXRotation(int rotation)
        {
            _xRotation = rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Updates the X-axis rotation (from a float input) and recalculates the total rotation
        /// offset.
        /// </summary>
        /// <param name="rotation">The float input value (cast to int) to be multiplied by <see
        /// cref="_angleFactor"/> for X-axis rotation.</param>
        public void UpdateXRotation(float rotation)
        {
            _xRotation = (int)rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Updates the Y-axis rotation and recalculates the total rotation offset.
        /// </summary>
        /// <param name="rotation">The input value to be multiplied by <see cref="_angleFactor"/>
        /// for Y-axis rotation.</param>
        public void UpdateYRotation(int rotation)
        {
            _yRotation = rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Updates the Y-axis rotation (from a float input) and recalculates the total rotation
        /// offset.
        /// </summary>
        /// <param name="rotation">The float input value (cast to int) to be multiplied by <see
        /// cref="_angleFactor"/> for Y-axis rotation.</param>
        public void UpdateYRotation(float rotation)
        {
            _yRotation = (int)rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Updates the Z-axis rotation and recalculates the total rotation offset.
        /// </summary>
        /// <param name="rotation">The input value to be multiplied by <see cref="_angleFactor"/>
        /// for Z-axis rotation.</param>
        public void UpdateZRotation(int rotation)
        {
            _zRotation = rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Updates the Z-axis rotation (from a float input) and recalculates the total rotation
        /// offset.
        /// </summary>
        /// <param name="rotation">The float input value (cast to int) to be multiplied by <see
        /// cref="_angleFactor"/> for Z-axis rotation.</param>
        public void UpdateZRotation(float rotation)
        {
            _zRotation = (int)rotation * _angleFactor;
            UpdateRotationOffset();
        }

        /// <summary>
        /// Recalculates the combined rotation offset based on the current X, Y, and Z rotation
        /// values.
        /// </summary>
        public void UpdateRotationOffset()
        {
            _rotationOffset = Quaternion.Euler(_xRotation, _yRotation, _zRotation);
        }
    }
}
