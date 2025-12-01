// <copyright file="PlaceFromObject.cs" company="Google LLC">
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

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Positions and rotates this GameObject (e.g., a UI panel) relative to a specified
    /// reference object, with optional smooth animation.
    /// </summary>
    public class PlaceFromObject : MonoBehaviour
    {
        /// <summary>
        /// The Transform to use as the reference point for positioning and rotation
        /// (e.g., the main camera or XR HMD.
        /// </summary>
        [Tooltip("The Transform to use as the reference point for positioning and rotation " +
                 "(e.g., the main camera or XR HMD).")]
        public Transform ReferenceObject;

        /// <summary>
        /// The desired position of this GameObject relative to the reference object's
        /// local space.
        /// </summary>
        [Tooltip("The desired position of this GameObject relative to the reference object's " +
                 "local space.")]
        public Vector3 RelativePos;

        /// <summary>If true, this GameObject will always face the reference object.</summary>
        [Tooltip("If true, this GameObject will always face the reference object.")]
        public bool FaceReference;

        /// <summary>
        /// If true, this GameObject's position will be constrained to the XZ plane
        /// relative to the reference's Y position.
        /// </summary>
        [Tooltip("If true, this GameObject's position will be constrained to the XZ plane " +
                 "relative to the reference's Y position.")]
        public bool LockOnXZ;

        /// <summary>
        /// If true and _lockOnXZ is true, the initial XZ distance from the reference will
        /// be maintained.
        /// </summary>
        [Tooltip("If true and _lockOnXZ is true, the initial XZ distance from the reference will " +
                 "be maintained.")]
        public bool PreserveDistance;

        /// <summary>If true, this GameObject's local X scale will be mirrored.</summary>
        [Tooltip(
            "If true, this GameObject's local X scale will be mirrored (e.g., to flip an avatar).")]
        public bool Mirror;

        [Header("Place automatically")]

        /// <summary>
        /// If true, the GameObject will be placed immediately when the scene starts.
        /// </summary>
        [Tooltip("If true, the GameObject will be placed immediately when the scene starts.")]
        public bool PlaceAtStart;

        /// <summary>
        /// If true, the GameObject will be placed on the very first Update frame.
        /// </summary>
        [Tooltip("If true, the GameObject will be placed on the very first Update frame.")]
        public bool PlaceOnFirstUpdate;

        /// <summary>If true, the GameObject will be placed on every Update frame.</summary>
        [Tooltip("If true, the GameObject will be placed on every Update frame.")]
        public bool PlaceOnUpdate;

        [Header("Animation Settings")]

        /// <summary>
        /// If true, the GameObject will smoothly animate to its target position/rotation
        /// when PlaceNow is called.
        /// </summary>
        [Tooltip("If true, the GameObject will smoothly animate to its target position/rotation " +
                 "when PlaceNow is called.")]
        public bool AnimateOnPlace = false;

        /// <summary>Duration of the placement animation in seconds.</summary>
        [Tooltip("Duration of the placement animation in seconds.")]
        public float AnimationDuration = 0.5f;

        /// <summary>
        /// Curve to control the animation's easing (e.g., EaseInOut for smooth start/end).
        /// </summary>
        [Tooltip("Curve to control the animation's easing (e.g., EaseInOut for smooth start/end).")]
        public AnimationCurve AnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Event invoked when the GameObject has been placed (immediately or after animation).
        /// </summary>
        public UnityEvent OnPlaced;

        /// <summary>Indicates if the first update has been processed.</summary>
        private bool _firstUpdateDone;

        /// <summary>
        /// Immediately places or animates this GameObject relative to the reference object,
        /// applying all configured positioning and rotation rules.
        /// </summary>
        public void PlaceNow()
        {
            if (ReferenceObject == null)
            {
                Debug.LogWarning("PlaceFromObject: Cannot place, reference object is null. " +
                                     "Please assign it in the inspector.",
                                 this);
                return;
            }

            (Vector3 targetPos, Quaternion targetRot) = CalculateTargetTransform();

            if (AnimateOnPlace && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();  // Stop any ongoing animation to start a new one cleanly
                StartCoroutine(
                    AnimatePlacement(transform.position, transform.rotation, targetPos, targetRot));
            }
            else
            {
                // Instant placement (no animation)
                transform.position = targetPos;
                transform.rotation = targetRot;
                OnPlaced.Invoke();  // Invoke event immediately for instant placement
            }
        }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Performs initial setup, placement, and mirroring if configured.
        /// </summary>
        private void Start()
        {
            if (ReferenceObject == null)
            {
                Debug.LogError(
                    "PlaceFromObject: Reference Object is not assigned. Disabling script.", this);
                enabled = false;
                return;
            }

            if (PlaceAtStart)
            {
                PlaceNow();
            }

            if (Mirror)
            {
                // Ensure mirroring happens only once or is managed externally if
                // transform.localScale.x can change
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),
                                                   transform.localScale.y, transform.localScale.z);
            }
        }

        /// <summary>
        /// Called once per frame. Handles placement based on configured update options.
        /// </summary>
        private void Update()
        {
            if (PlaceOnUpdate)
            {
                PlaceNow();
            }
            else if (PlaceOnFirstUpdate && !_firstUpdateDone)
            {
                PlaceNow();
                _firstUpdateDone = true;
            }
        }

        /// <summary>
        /// Calculates the target position and rotation based on reference object and settings.
        /// </summary>
        /// <returns>A tuple containing the calculated target position and rotation.</returns>
        private (Vector3 targetPos, Quaternion targetRot) CalculateTargetTransform()
        {
            if (ReferenceObject == null)
            {
                Debug.LogWarning("PlaceFromObject: Reference object is null during calculation. " +
                                     "Returning current transform.",
                                 this);
                return (transform.position, transform.rotation);
            }

            // Calculate base position relative to reference object
            Vector3 calculatedPosition =
                ReferenceObject.position + (ReferenceObject.rotation * RelativePos);
            Quaternion calculatedRotation =
                transform.rotation;  // Default: maintain current rotation

            if (LockOnXZ)
            {
                // Keep the Y-position from the newPosition but align XZ to the reference's Y-plane.
                calculatedPosition.y = ReferenceObject.position.y;

                if (PreserveDistance)
                {
                    // This logic assumes _relativePos is the desired distance.
                    // It projects the desired relative position onto the XZ plane relative to the
                    // reference.
                    Vector3 displacementFromRefXZ = (calculatedPosition - ReferenceObject.position);
                    displacementFromRefXZ.y = 0;  // Flatten to XZ plane

                    float givenDistanceXZ = new Vector3(RelativePos.x, 0, RelativePos.z)
                                                .magnitude;  // Desired XZ distance
                    float currentDistanceXZ =
                        displacementFromRefXZ.magnitude;  // Current XZ distance

                    if (currentDistanceXZ > 0.001f)
                    {
                        calculatedPosition = ReferenceObject.position +
                                             (displacementFromRefXZ.normalized * givenDistanceXZ);
                    }
                    else
                    {
                        // If current XZ distance is near zero, and we want to preserve distance,
                        // project _relativePos onto XZ and add it.
                        calculatedPosition =
                            ReferenceObject.position + new Vector3(RelativePos.x, 0, RelativePos.z);
                    }
                }
            }

            if (FaceReference)
            {
                // Calculate rotation to face the reference object, ensuring only Y-axis rotation.
                // We want the object's forward (Z) to point towards the reference object.
                Vector3 directionToRef = ReferenceObject.position - calculatedPosition;
                directionToRef.y = 0;                       // Lock rotation to Y-axis
                if (directionToRef.sqrMagnitude > 0.0001f)
                {
                    calculatedRotation = Quaternion.LookRotation(directionToRef.normalized);
                }
            }

            /* If _faceReference is false, the object retains its current rotation,
            or you might want it to match the reference's forward rotation.
            For this version, it keeps its own rotation if not facing the reference. */

            return (calculatedPosition, calculatedRotation);
        }

        /// <summary>
        /// Coroutine to smoothly animate the GameObject to a target position and rotation.
        /// </summary>
        /// <param name="startPos">Starting position for the animation.</param>
        /// <param name="startRot">Starting rotation for the animation.</param>
        /// <param name="endPos">Target position for the animation.</param>
        /// <param name="endRot">Target rotation for the animation.</param>
        /// <returns>The IEnumerator for the Coroutine.</returns>
        private IEnumerator AnimatePlacement(Vector3 startPos, Quaternion startRot, Vector3 endPos,
                                             Quaternion endRot)
        {
            float timer = 0f;
            while (timer < AnimationDuration)
            {
                float progress = timer / AnimationDuration;
                float curveValue = AnimationCurve.Evaluate(progress);

                transform.position = Vector3.Lerp(startPos, endPos, curveValue);
                transform.rotation = Quaternion.Slerp(startRot, endRot, curveValue);

                timer += Time.deltaTime;
                yield return null;
            }

            // Ensure final position/rotation is exact after animation
            transform.position = endPos;
            transform.rotation = endRot;

            OnPlaced.Invoke();  // Invoke event only once animation is complete
        }
    }
}
