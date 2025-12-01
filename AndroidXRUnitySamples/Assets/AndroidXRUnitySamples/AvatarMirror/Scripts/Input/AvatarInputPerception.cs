// <copyright file="AvatarInputPerception.cs" company="Google LLC">
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
    /// Provides avatar input data from perception-based tracking components, implementing the <see
    /// cref="IAvatarInput"/> interface.
    /// </summary>
    public class AvatarInputPerception : MonoBehaviour, IAvatarInput
    {
        /// <summary>The Transform that defines the avatar's overall reference frame.</summary>
        [Tooltip("The Transform that defines the avatar's overall reference frame.")]
        [SerializeField] public Transform Reference;

        /// <summary>
        /// The initial height offset (in meters) for the reference frame above the camera.
        /// </summary>
        [Tooltip("The initial height offset (in meters) for the reference frame above the camera.")]
        [SerializeField] public float RefHeightOverHead = 0.3f;

        /// <summary>The current eye tracking data.</summary>
        private static AvatarEyeData _avatarEyeData;

        /// <summary>The current head tracking data.</summary>
        private static AvatarHeadData _avatarHeadData;

        /// <summary>The current hand tracking data.</summary>
        private static AvatarHandsData _avatarHandData;

        /// <summary>The current body tracking data.</summary>
        private static AvatarBodyData _avatarBodyData;

        /// <summary>The current face tracking data.</summary>
        private static AvatarFaceData _avatarFaceData;

        /// <summary>If true, the reference frame will be reset when the script starts.</summary>
        [Tooltip("If true, the reference frame will be reset when the script starts.")]
        [SerializeField] private bool _resetReferenceAtStart;

        /// <summary>
        /// If true, the reference frame will be reset during the first Update call.
        /// </summary>
        [Tooltip("If true, the reference frame will be reset during the first Update call.")]
        [SerializeField] private bool _resetReferenceAtFirstUpdate;

        /// <summary>
        /// Indicates if the first update has completed, used for one-time resets.
        /// </summary>
        private bool _firstUpdateDone;

        [Space]

        [Tooltip("The AvatarEyeTrackingInput component providing eye data.")]
        [SerializeField] private AvatarEyeTracking _avatarEyeTracking;

        [Tooltip("The AvatarHeadTracking component providing head data.")]
        [SerializeField] private AvatarHeadTracking _avatarHeadTracking;

        [Tooltip("The AvatarHandTracking component providing hand data.")]
        [SerializeField] private AvatarHandTracking _avatarHandTracking;

        [Tooltip("The AvatarFaceTracking component providing face data.")]
        [SerializeField] private AvatarFaceTracking _avatarFaceTracking;

        [Tooltip("The AvatarBodyTracking component providing body data.")]
        [SerializeField] private AvatarBodyTracking _avatarBodyTracking;

        /// <summary>Reference to the scene's main camera.</summary>
        private Camera _mainCamera;

        //// --- Avatar Data Properties ---

        /// <summary>Gets or sets the current eye tracking data.</summary>
        public static AvatarEyeData EyeData
        {
            get => _avatarEyeData;
            set => _avatarEyeData = value;
        }

        /// <summary>Gets or sets the current head tracking data.</summary>
        public static AvatarHeadData HeadData
        {
            get => _avatarHeadData;
            set => _avatarHeadData = value;
        }

        /// <summary>Gets or sets the current hand tracking data.</summary>
        public static AvatarHandsData HandData
        {
            get => _avatarHandData;
            set => _avatarHandData = value;
        }

        /// <summary>Gets or sets the current body tracking data.</summary>
        public static AvatarBodyData BodyData
        {
            get => _avatarBodyData;
            set => _avatarBodyData = value;
        }

        /// <summary>Gets or sets the current face tracking data.</summary>
        public static AvatarFaceData FaceData
        {
            get => _avatarFaceData;
            set => _avatarFaceData = value;
        }

        //// --- End Avatar Data Properties ---

        /// <summary>
        /// Resets the position and rotation of the avatar's reference frame.
        /// The position is set relative to the main camera, with an optional height offset.
        /// The rotation is set to match the camera's forward direction on the Y-plane.
        /// </summary>
        public void ResetReferenceFrame()
        {
            if (_mainCamera == null || Reference == null)
            {
                Debug.LogWarning(
                    "Cannot reset reference frame: Main Camera or Reference Transform is null.");
                return;
            }

            // Set the reference position: camera's X and Z, plus a height offset.
            Reference.position = new Vector3(_mainCamera.transform.position.x,
                                              _mainCamera.transform.position.y + RefHeightOverHead,
                                              _mainCamera.transform.position.z);

            // Set the reference rotation: maintain horizontal forward direction of the camera.
            Vector3 cameraForwardFlat =
                new Vector3(_mainCamera.transform.forward.x, 0f, _mainCamera.transform.forward.z)
                    .normalized;
            Reference.rotation = Quaternion.LookRotation(cameraForwardFlat, Vector3.up);

            Debug.Log("Reference frame reset successfully.");
        }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the main camera reference and performs an initial reference frame reset if
        /// configured.
        /// </summary>
        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("AvatarInputPerception requires a main camera (tagged " +
                                   "'MainCamera') in the scene. Disabling.",
                               this);
                enabled = false;
                return;
            }

            if (Reference == null)
            {
                Debug.LogError("AvatarInputPerception requires a 'Reference' Transform to be " +
                                   "assigned. Disabling.",
                               this);
                enabled = false;
                return;
            }

            if (_resetReferenceAtStart)
            {
                ResetReferenceFrame();
            }
        }

        /// <summary>
        /// Called once per frame.
        /// Performs a one-time reference frame reset if configured and updates all avatar data from
        /// tracking components.
        /// </summary>
        private void Update()
        {
            if (_resetReferenceAtFirstUpdate && !_firstUpdateDone)
            {
                ResetReferenceFrame();
                _firstUpdateDone = true;
            }

            // Assign data from each tracking component to the static properties
            if (_avatarHeadTracking != null)
            {
                HeadData = _avatarHeadTracking.HeadData;
            }

            if (_avatarFaceTracking != null)
            {
                FaceData = _avatarFaceTracking.FaceData;
            }

            if (_avatarHandTracking != null)
            {
                HandData = _avatarHandTracking.HandData;
            }

            if (_avatarEyeTracking != null)
            {
                EyeData = _avatarEyeTracking.EyeData;
            }

            if (_avatarBodyTracking != null)
            {
                BodyData = _avatarBodyTracking.BodyData;
            }
        }
    }
}
