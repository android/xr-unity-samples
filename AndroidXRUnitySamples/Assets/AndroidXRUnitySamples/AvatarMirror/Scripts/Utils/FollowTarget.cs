// <copyright file="FollowTarget.cs" company="Google LLC">
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
    /// Smoothly moves and optionally rotates a targetTransform towards this GameObject
    /// using an exponential blending approach.
    /// </summary>
    [ExecuteInEditMode]
    public class FollowTarget : MonoBehaviour
    {
        /// <summary>The Transform this object will move towards.</summary>
        [Tooltip("The Transform this object will move towards.")]
        public Transform TargetTransform;

        /// <summary>
        /// The blending factor for position. Higher values mean faster movement towards
        /// the target.
        /// </summary>
        [Tooltip("The blending factor for position. Higher values mean faster movement towards " +
                 "the target.")]
        [Range(0.01f, 1f)]
        public float PositionBlendFactor = 0.1f;

        /// <summary>
        /// If true, the object will also smoothly rotate towards the target's rotation.
        /// </summary>
        [Tooltip("If true, the object will also smoothly rotate towards the target's rotation.")]
        public bool SmoothRotation = true;

        /// <summary>
        /// If true, the offset will be applied in the target's local space; otherwise, in world
        /// space.
        /// </summary>
        [Tooltip("The blending factor for rotation. Higher values mean faster rotation towards " +
                 "the target.")]
        [Range(0.01f, 1f)]
        public float RotationBlendFactor = 0.1f;

        [Space]

        /// <summary>An optional offset applied to the target's position.</summary>
        [Tooltip("An optional offset applied to the target's position.")]
        [SerializeField] private Vector3 _offset = Vector3.zero;

        /// <summary>
        /// If true, the offset will be applied in the target's local space; otherwise, in world
        /// space.
        /// </summary>
        [Tooltip("If true, the offset will be applied in the target's local space; otherwise, in " +
                 "world space.")]
        [SerializeField] private bool _offsetLocalSpace;

        /// <summary>
        /// Called once per frame. Updates the position and rotation of this GameObject.
        /// </summary>
        void Update()
        {
            // Only proceed if a targetTransform is assigned
            if (TargetTransform == null)
            {
                Debug.LogWarning(
                    "FollowTarget script on " + gameObject.name +
                        " has no targetTransform assigned. Please assign one in the inspector.",
                    this);
                return;
            }

            // Calculate the target position with or without offset
            Vector3 desiredPosition;
            if (_offsetLocalSpace)
            {
                desiredPosition = transform.position + transform.rotation * _offset;
            }
            else
            {
                desiredPosition = transform.position + _offset;
            }

            // Smoothly move towards the desired position
            // Lerp's 't' value is clamped between 0 and 1. Multiplying by Time.deltaTime
            // makes the smoothing frame-rate independent.
            TargetTransform.position =
                Vector3.Lerp(TargetTransform.position, desiredPosition, PositionBlendFactor);

            // Smoothly rotate towards the target's rotation (optional)
            if (SmoothRotation)
            {
                TargetTransform.rotation = Quaternion.Slerp(
                    TargetTransform.rotation, transform.rotation, RotationBlendFactor);
            }
        }
    }
}
