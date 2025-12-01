// <copyright file="FollowParentWithOffsetAndFaceCamera.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    /// Keeps this GameObject at a fixed world Y-offset from a target parent object,
    /// following the parent's X and Z world position.
    /// It also rotates, blending between facing the main camera and the parent's rotation,
    /// with adjustable weights for each axis. Parent rotation can also be dampened/amplified.
    /// </summary>
    public class FollowParentWithOffsetAndFaceCamera : MonoBehaviour
    {
        [Header("Target References")]
        [Tooltip("The transform of the object to follow. If null, tries to use transform.parent.")]
        [SerializeField]
        private Transform _parentTransform;

        [Tooltip("The camera transform to face. If null, tries to use Camera.main.")]
        [SerializeField]
        private Transform _cameraTransform;

        [Header("Following Behaviour")]
        [Tooltip("The fixed offset in world Y units above or below the parent.")]
        [SerializeField]
        private float _worldYOffset = 1.5f;

        [Header("Rotation Blending")]
        [Tooltip("Weights for blending between parent rotation"
               + " and camera-facing rotation for X, Y, and Z axes."
               + " 0 = purely parent, 1 = purely camera.")]
        [SerializeField]
        private Vector3 _rotationBlendWeights =
                new Vector3(0.5f, 0.5f, 0.5f); // Default to equal blend

        [Tooltip("Factor to dampen or amplify the parent's rotation"
               + " for X, Y, and Z axes. 1 = no change, <1 = dampen, >1 = amplify.")]
        [SerializeField]
        private Vector3 _parentRotationInfluence = Vector3.one; // Default to no change (1,1,1)

        private Quaternion _initialLocalRotation; // To track the original offset from parent

        private void Start()
        {
            if (_parentTransform == null)
            {
                _parentTransform = transform.parent;
                if (_parentTransform == null)
                {
                    Debug.LogError(
                            $"[{gameObject.name}] Follower script needs a parent or a "
                          + $"'Parent Transform' assigned!",
                            this);
                    enabled = false;
                    return;
                }

                Debug.LogWarning(
                        $"[{gameObject.name}] Follower script using transform.parent as "
                      + $"target. Assign 'Parent Transform' explicitly if this is not intended.",
                        this);
            }

            // If cameraTransform is not assigned, try to find the main camera
            if (_cameraTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    _cameraTransform = mainCamera.transform;
                }
                else
                {
                    Debug.LogError(
                            $"[{gameObject.name}] Follower script needs a camera reference."
                          + $" Assign 'Camera Transform' or ensure"
                          + $" a Camera is tagged 'MainCamera'.", this);
                    enabled = false;
                    return;
                }
            }

            // Store the initial local rotation relative to the parent
            // Used to apply the parent's rotation influence around its original orientation
            _initialLocalRotation = Quaternion.Inverse(_parentTransform.rotation)
                                  * transform.rotation;
        }

        private void LateUpdate()
        {
            if (_parentTransform == null || _cameraTransform == null)
            {
                return;
            }

            // Position Update
            Vector3 parentPosition = _parentTransform.position;
            var targetPosition = new Vector3(
                    parentPosition.x,
                    parentPosition.y + _worldYOffset,
                    parentPosition.z);

            transform.position = targetPosition;

            // Rotation Blending with Parent Influence

            // 1. Calculate camera-facing rotation (world space)
            Vector3 currentPosition = transform.position;
            Vector3 cameraPosition = _cameraTransform.position;
            Vector3 directionToCamera = cameraPosition - currentPosition;

            // Keep it upright for camera-facing on the Y-axis
            Vector3 directionXZ = new Vector3(directionToCamera.x, 0f, directionToCamera.z);
            Quaternion cameraFacingRotation = Quaternion.identity;
            if (directionXZ.sqrMagnitude > 0.0001f)
            {
                cameraFacingRotation = Quaternion.LookRotation(directionXZ,
                        Vector3.up);
            }

            // 2. Calculate parent's influenced rotation (world space)
            // Get the parent's current world rotation
            Quaternion parentActualWorldRotation = _parentTransform.rotation;

            // Apply dampening/amplification to the parent's rotation
            // We'll get the parent's rotation as Euler angles, apply the influence,
            // and then convert back to a Quaternion.
            Vector3 parentActualEuler = parentActualWorldRotation.eulerAngles;
            Vector3 initialLocalEuler = _initialLocalRotation.eulerAngles;

            float rX = initialLocalEuler.x + (parentActualEuler.x - initialLocalEuler.x)
                     * _parentRotationInfluence.x;
            float rY = initialLocalEuler.y + (parentActualEuler.y - initialLocalEuler.y)
                     * _parentRotationInfluence.y;
            float rZ =
            initialLocalEuler.z + (parentActualEuler.z - initialLocalEuler.z)
                  * _parentRotationInfluence.z;

            Vector3 parentInfluencedEuler = new Vector3(rX, rY, rZ);
            Quaternion parentInfluencedRotation = Quaternion.Euler(parentInfluencedEuler);

            // 3. Blend the rotations
            Vector3 parentInfluencedEulerForBlend = parentInfluencedRotation.eulerAngles;
            Vector3 cameraEulerForBlend = cameraFacingRotation.eulerAngles;

            // Apply blending to each Euler axis
            // Mathf.LerpAngle handles the 360/0 wrap-around for smooth interpolation
            float blendedX = Mathf.LerpAngle(parentInfluencedEulerForBlend.x,
                    cameraEulerForBlend.x, _rotationBlendWeights.x);
            float blendedY = Mathf.LerpAngle(parentInfluencedEulerForBlend.y,
                    cameraEulerForBlend.y, _rotationBlendWeights.y);
            float blendedZ = Mathf.LerpAngle(parentInfluencedEulerForBlend.z,
                    cameraEulerForBlend.z, _rotationBlendWeights.z);

            Quaternion blendedRotation = Quaternion.Euler(blendedX, blendedY, blendedZ);

            transform.rotation = blendedRotation;
        }
    }
}
