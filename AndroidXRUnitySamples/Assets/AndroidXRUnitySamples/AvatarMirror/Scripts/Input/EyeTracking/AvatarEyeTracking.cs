// <copyright file="AvatarEyeTracking.cs" company="Google LLC">
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
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.Features.Android;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Manages eye tracking data from <see cref="ARFaceManager"/> and applies configured filtering.
    /// </summary>
    public class AvatarEyeTracking : MonoBehaviour
    {
        /// <summary>Configuration settings for eye tracking, including filtering.</summary>
        [Tooltip("Configuration settings for eye tracking, including filtering.")]
        public AvatarEyeTrackingSettings Settings;

        /// <summary>The current eye data, populated from the <see cref="ARFaceManager"/>.</summary>
        private AvatarEyeData _avatarEyeData = new AvatarEyeData();

        /// <summary>The exponential filter used for smoothing eye position, if enabled.</summary>
        private ExponentialFilter _exponentialFilter;

        /// <summary>
        /// The ARFaceManager component responsible for detecting and tracking faces.
        /// </summary>
        [Tooltip("The ARFaceManager component responsible for detecting and tracking faces.")]
        [SerializeField] private ARFaceManager _faceManager;

        /// <summary>Reference to the scene's main camera.</summary>
        private Camera _mainCamera;

        /// <summary>Gets the current eye data, updated each frame.</summary>
        public AvatarEyeData EyeData => _avatarEyeData;

        /// <summary>
        /// Gets or sets the <see cref="ARFaceManager"/> component.
        /// </summary>
        public ARFaceManager FaceManager
        {
            private get => _faceManager;
            set => _faceManager = value;
        }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the main camera reference and the exponential filter if configured.
        /// Deactivates the component in the editor, as eye tracking requires a device.
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Class AvatarEyeTracking is designed for device runtime and will be " +
                      "deactivated in the editor.");
            enabled = false;
#else
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("AvatarEyeTracking requires a main camera (tagged 'MainCamera') " +
                                   "in the scene. Disabling.",
                               this);
                enabled = false;
                return;
            }

            if (Settings.FilterType == FilterEnum.Exponential)
            {
                _exponentialFilter = new ExponentialFilter(Settings.ExpFilterSettings);
            }
#endif
        }

        /// <summary>
        /// Called once per frame.
        /// Updates the <see cref="AvatarEyeData"/> from the <see cref="ARFaceManager"/>
        /// and applies filtering if enabled.
        /// </summary>
        private void Update()
        {
            // Calculate the inverse of the main camera's rotation to convert eye poses to
            // head-relative space.
            Quaternion invHeadRot = Quaternion.Inverse(_mainCamera.transform.rotation);

            // Iterate through all currently tracked faces. We typically only care about the first
            // one for eye tracking.
            foreach (ARFace face in _faceManager.trackables)
            {
#pragma warning disable CS0618 // Temp solution for Unity AXR 1.1.0-pre.1 migration.
                // Attempt to get the Android OpenXR specific face tracking states for more granular
                // validity checks.
                if (face.TryGetAndroidOpenXRFaceTrackingStates(
                        out AndroidOpenXRFaceTrackingStates states))
                {
                    // Check if the left eye pose is valid before using its data.
                    if ((states & AndroidOpenXRFaceTrackingStates.LeftEyePoseValid) != 0)
                    {
                        Quaternion eyeL = face.leftEye.rotation;
                        _avatarEyeData.LeftEyeRotation = invHeadRot * eyeL;
                    }

                    // Check if the right eye pose is valid before using its data.
                    if ((states & AndroidOpenXRFaceTrackingStates.RightEyePoseValid) != 0)
                    {
                        Quaternion eyeR = face.rightEye.rotation;
                        _avatarEyeData.RightEyeRotation = invHeadRot * eyeR;
                    }

                    _avatarEyeData.GazeRotation = Quaternion.Slerp(
                        _avatarEyeData.LeftEyeRotation, _avatarEyeData.RightEyeRotation, 0.5f);
#pragma warning restore CS0618 // Temp solution for Unity AXR 1.1.0-pre.1 migration.
                }

                // Break after processing the first face, as we typically only need one for avatar
                // eye tracking.
                break;
            }

            // Apply filtering to the gaze position if the exponential filter is enabled.
            if (Settings.FilterType == FilterEnum.Exponential && _exponentialFilter != null)
            {
                _avatarEyeData.GazePosition =
                    _exponentialFilter.Filter(_avatarEyeData.GazePosition);
            }
        }
    }

    /// <summary>
    /// Settings for avatar eye tracking, including filtering options.
    /// </summary>
    [System.Serializable]
    public class AvatarEyeTrackingSettings
    {
        [Header("Filtering")]  // Corrected header for clarity

        /// <summary>
        /// The type of filter to apply to the eye data (pick 'No' for no filtering.
        /// </summary>
        [Tooltip("The type of filter to apply to the eye data (pick 'No' for no filtering).")]
        public FilterEnum FilterType = FilterEnum.No;

        /// <summary>
        /// Settings for the exponential filter, if 'Exponential' filter type is selected.
        /// </summary>
        [Tooltip("Settings for the exponential filter, if 'Exponential' filter type is selected.")]
        public ExponentialFilterSettingsV3 ExpFilterSettings;
    }
}
