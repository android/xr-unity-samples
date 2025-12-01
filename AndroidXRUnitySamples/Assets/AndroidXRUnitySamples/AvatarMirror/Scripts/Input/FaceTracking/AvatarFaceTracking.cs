// <copyright file="AvatarFaceTracking.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Google.XR.Extensions;
    using UnityEngine;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Demonstrates the face tracking feature for avatar facial blendshapes.
    /// </summary>
    [RequireComponent(typeof(XRFaceTrackingManager))]
    public class AvatarFaceTracking : MonoBehaviour
    {
        /// <summary>List of parameter names from XRFaceParameterIndices.</summary>
        public string[] ParamNames;

        [SerializeField] private XRFaceTrackingManager _faceManager;

        /// <summary>The current face data.</summary>
        private AvatarFaceData _avatarFaceData = new AvatarFaceData();

        /// <summary>Gets the current face data.</summary>
        public AvatarFaceData FaceData => _avatarFaceData;

        private void Awake()
        {
            ParamNames = Enum.GetNames(typeof(XRFaceParameterIndices));
            _faceManager = GetComponent<XRFaceTrackingManager>();

#if UNITY_EDITOR
            Debug.Log("Class AvatarFaceTracking cannot run in editor. Deactivating.");
            _faceManager.enabled = false;
            enabled = false;
#endif
        }

        private void Update()
        {
            if (XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled == null)
            {
                Debug.Log("XrInstance hasn't been initialized.");
                return;
            }

            if (!XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled.Value)
            {
                Debug.Log("XR_ANDROID_face_tracking is not enabled.");
                return;
            }

            // Directly assign the Parameters array from the face manager to avatarFaceData.
            _avatarFaceData.Parameters = _faceManager.Face.Parameters;
        }
    }
}
