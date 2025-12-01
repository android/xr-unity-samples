// <copyright file="AvatarAnimationSettings.cs" company="Google LLC">
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

using System;
using UnityEngine;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Controls the animation behavior and input source selection for an avatar model.
    /// This script configures how an avatar receives and processes data for its animation.
    /// </summary>
    public class AvatarAnimationSettings : MonoBehaviour
    {
        /// <summary>
        /// The root GameObject of the avatar hierarchy, used as the origin for tracking " +
        /// transformations.
        /// </summary>
        [Tooltip("The root GameObject of the avatar hierarchy, used as the origin for tracking " +
                 "transformations.")]
        [SerializeField] public GameObject AvatarOrigin;

        /// <summary>
        /// If true, face blendshape data will be retargeted to match the avatar's blendshapes.
        /// </summary>
        [Tooltip(
            "If true, face blendshape data will be retargeted to match the avatar's blendshapes.")]
        [SerializeField] public bool RetargetBlendshapes;

        /// <summary>
        /// The number of blendshapes to skip when applying blendshape data to the model's
        /// face (e.g., for offset mapping).
        /// </summary>
        [Tooltip("The number of blendshapes to skip when applying blendshape data to the model's " +
                 "face (e.g., for offset mapping).")]
        [SerializeField] public int BlendshapesToSkip;

        /// <summary>
        /// A mapping array where index is the source blendshape and value is the target
        /// blendshape index on the avatar model.
        /// </summary>
        [Tooltip("A mapping array where index is the source blendshape and value is the target " +
                 "blendshape index on the avatar model.")]
        [SerializeField] public int[] BlendShapeMap =
                new int[AvatarFaceData.OpenXRFaceBlendshapeCount];

        [Space]

        /// <summary>
        /// If true, only the rotation of the hands is applied, ignoring positional data
        /// from the input.
        /// </summary>
        [Tooltip("If true, only the rotation of the hands is applied, ignoring positional data " +
                 "from the input.")]
        [SerializeField] public bool HandOnlyRotation;

        /// <summary>
        /// If true, animation updates for metacarpal bones will be skipped.
        /// </summary>
        [Tooltip("If true, animation updates for metacarpal bones will be skipped.")]
        [SerializeField] public bool SkipMetacarpals;

        [Space]

        /// <summary>
        /// Whether to utilize body tracking data to animate the avatar's torso and limbs.
        /// </summary>
        [Tooltip("Whether to utilize body tracking data to animate the avatar's torso and limbs.")]
        [SerializeField] public bool UseBodyTracking;

        /// <summary>
        /// If true, only the rotation of the body joints is applied, ignoring positional data.
        /// </summary>
        [Tooltip(
            "If true, only the rotation of the body joints is applied, ignoring positional data.")]
        [SerializeField] public bool BodyOnlyRotation;

        /// <summary>
        /// If true, the body tracking data will be flipped along a specified axis.
        /// </summary>
        [Tooltip("If true, the body tracking data will be flipped along a specified axis.")]
        [SerializeField] public bool FlipBody;

        /// <summary>
        /// The axis along which to flip the body tracking data if 'Flip Body' is enabled
        /// (0=X, 1=Y, 2=Z, 3=QZ).
        /// </summary>
        [Tooltip("The axis along which to flip the body tracking data if 'Flip Body' is enabled " +
                 "(0=X, 1=Y, 2=Z, 3=QZ).")]
        [SerializeField] public int FlipBodyAxis;

        [Space]

        /// <summary>
        /// Container for various animation-related settings, including blending factors.
        /// </summary>
        [Tooltip("Container for various animation-related settings, including blending factors.")]
        [SerializeField] public AnimationSettings AnimationSettings;

        private bool _allowRelativeMotion = false;  // Initialized here for direct default value.

        /// <summary>
        /// Gets or sets a value indicating whether the avatar is allowed to move relative to its
        /// base location, or if its position should remain fixed.
        /// </summary>
        public bool AllowRelativeMotion
        {
            get => _allowRelativeMotion;
            set => _allowRelativeMotion = value;
        }
    }

    /// <summary>
    /// Contains settings for blending different animation data streams for an avatar.
    /// </summary>
    [Serializable]
    public class BlendingSettings
    {
        [Tooltip("The blending factor for the eyes' rotation (0 = no input, 1 = full input).")]
        [Range(0f, 1f)]
        [SerializeField] private float _eyeBlendFactor = 1f;

        [Tooltip("The blending factor specifically for individual finger joint rotations (0 = no " +
                 "input, 1 = full input).")]
        [Range(0f, 1f)]
        [SerializeField] private float _fingerBlendFactor = 1f;

        [Tooltip("The blending factor for face blendshapes (0 = no input, 1 = full input).")]
        [Range(0f, 1f)]
        [SerializeField] private float _faceBlendshapeBlendFactor = 1f;

        /// <summary>The eye animation blending factor.</summary>
        public float EyeBlendFactor => _eyeBlendFactor;

        /// <summary>The fingers animation blending factor.</summary>
        public float FingerBlendFactor => _fingerBlendFactor;

        /// <summary>The face animation blending factor.</summary>
        public float FaceBlendshapeBlendFactor => _faceBlendshapeBlendFactor;
    }

    /// <summary>
    /// Contains general animation settings for an avatar.
    /// </summary>
    [Serializable]
    public class AnimationSettings
    {
        [Tooltip("Settings for blending animation data from various input sources.")]
        [SerializeField] private BlendingSettings _blendingSettings;

        /// <summary>Public property to access blending settings.</summary>
        public BlendingSettings BlendingSettings => _blendingSettings;
    }
}
