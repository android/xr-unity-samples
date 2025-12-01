// <copyright file="ShoulderTwistCorrections.cs" company="Google LLC">
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
    /// Applies corrective rotations to "twist" bones (e.g., in the upper arm)
    /// based on the primary rotation of a shoulder joint.
    /// </summary>
    public class ShoulderTwistCorrections : MonoBehaviour
    {
        [Tooltip("The main shoulder GameObject whose rotation drives the twist corrections.")]
        [SerializeField] private GameObject _shoulder;

        [Tooltip("The first twist correction GameObject (e.g., upper arm twist bone).")]
        [SerializeField] private GameObject _twist1;

        [Tooltip("The second twist correction GameObject.")]
        [SerializeField] private GameObject _twist2;

        [Tooltip("The third twist correction GameObject.")]
        [SerializeField] private GameObject _twist3;

        [Tooltip(
            "The local axis of the shoulder on which the primary rotation (twist) is computed.")]
        [SerializeField] private AnimationUtils.Axis _rotationAxis = AnimationUtils.Axis.Y;

        [Tooltip("The local axis on which the twist correction rotations are applied to the " +
                 "twist joints.")]
        [SerializeField] private AnimationUtils.Axis _correctionAxis = AnimationUtils.Axis.Y;

        /// <summary>Stores the previous calculated twist delta angle.</summary>
        private float _prevDelta;

        /// <summary>Stores the total accumulated twist delta angle.</summary>
        private float _deltaApplied;

        /// <summary>Counts the number of full turns detected in the twist rotation.</summary>
        private int _turns = 0;

        /// <summary>The starting local rotation of the shoulder.</summary>
        private Quaternion _shoulderStart;

        /// <summary>The starting local rotation of the first twist bone.</summary>
        private Quaternion _twist1Start;

        /// <summary>The starting local rotation of the second twist bone.</summary>
        private Quaternion _twist2Start;

        /// <summary>The starting local rotation of the third twist bone.</summary>
        private Quaternion _twist3Start;

        /// <summary>
        /// Initializes the starting local rotations of the shoulder and twist bones.
        /// </summary>
        private void Start()
        {
            if (_shoulder == null || _twist1 == null || _twist2 == null || _twist3 == null)
            {
                Debug.LogError("ShoulderTwistCorrections: All shoulder and twist GameObjects " +
                                   "must be assigned. Disabling script.",
                               this);
                enabled = false;
                return;
            }

            _shoulderStart = _shoulder.transform.localRotation;
            _twist1Start = _twist1.transform.localRotation;
            _twist2Start = _twist2.transform.localRotation;
            _twist3Start = _twist3.transform.localRotation;
        }

        /// <summary>
        /// Applies the shoulder twist corrections each frame.
        /// </summary>
        private void Update()
        {
            Vector3 ConvertAxisToVector(AnimationUtils.Axis axis)
            {
                Vector3 result;
                switch (axis)
                {
                    case AnimationUtils.Axis.X:
                        result = Vector3.right;
                        break;
                    case AnimationUtils.Axis.Y:
                        result = Vector3.up;
                        break;
                    case AnimationUtils.Axis.Z:
                        result = Vector3.forward;
                        break;
                    case AnimationUtils.Axis.negX:
                        result = Vector3.left;
                        break;
                    case AnimationUtils.Axis.negY:
                        result = Vector3.down;
                        break;
                    case AnimationUtils.Axis.negZ:
                        result = Vector3.back;
                        break;
                    default:
                        result = Vector3.zero;
                        break;
                }

                return result;
            }

            Vector3 rotationVector = ConvertAxisToVector(_rotationAxis);

            Vector3 secondCrossVector = (rotationVector == Vector3.up ||
                rotationVector == Vector3.down) ? Vector3.forward : Vector3.up;

            Vector3 secondVector = Vector3.Cross(rotationVector, secondCrossVector);
            Vector3 correctionVector = ConvertAxisToVector(_correctionAxis);

            Quaternion currentLocalRotation = _shoulder.transform.localRotation;
            Quaternion shoulderDelta = Quaternion.Inverse(_shoulderStart) * currentLocalRotation;

            (Quaternion twist, Quaternion swing) =
                AnimationUtils.DecomposeTwistSwing(shoulderDelta, rotationVector);

            float delta = Vector3.SignedAngle(secondVector, twist * secondVector, rotationVector);

            if (Mathf.Abs(delta - _prevDelta) > 180f)
            {
                if (delta < _prevDelta)
                {
                    _turns += 1;
                }
                else
                {
                    _turns -= 1;
                }
            }

            _prevDelta = delta;
            _deltaApplied = delta + 360f * _turns;

            _twist1.transform.localRotation =
                Quaternion.Euler(correctionVector * -_deltaApplied) * _twist1Start;
            _twist2.transform.localRotation =
                Quaternion.Euler(correctionVector * (-0.67f * _deltaApplied)) * _twist2Start;
            _twist3.transform.localRotation =
                Quaternion.Euler(correctionVector * (-0.33f * _deltaApplied)) * _twist3Start;
        }
    }
}
