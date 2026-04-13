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
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Demonstrates the face tracking feature for avatar facial blendshapes.
    /// </summary>
    public class AvatarFaceTracking : MonoBehaviour
    {
        /// <summary>List of parameter names from XRFaceParameterIndices.</summary>
        public string[] ParamNames;

        /// <summary>Face manager from AR Foundation.</summary>
        public ARFaceManager FaceManager = null;

        private ARFace _face;

        /// <summary>The current face data.</summary>
        private AvatarFaceData _avatarFaceData = new AvatarFaceData();

        /// <summary>Gets the current face data.</summary>
        public AvatarFaceData FaceData => _avatarFaceData;

        private void Awake()
        {
#if UNITY_EDITOR
            Debug.Log("Class AvatarFaceTracking cannot run in editor. Deactivating.");
            enabled = false;
#else
            FaceManager.enabled = true;
            FaceManager.trackablesChanged.AddListener(OnFaceChanged);
#endif
        }

        private void OnFaceChanged(ARTrackablesChangedEventArgs<ARFace> eventArgs)
        {
            foreach (ARFace face in eventArgs.added)
            {
                if (_face != face)
                {
                    _face = face;
                    face.updated += OnFaceUpdate;
                }
            }

            foreach (ARFace face in eventArgs.updated)
            {
                if (_face != face)
                {
                    _face = face;
                    face.updated += OnFaceUpdate;
                }
            }

            foreach (var facePair in eventArgs.removed)
            {
                if (_face != null && facePair.Key.Equals(_face.trackableId))
                {
                    _face = null;
                }
            }
        }

        private void OnFaceUpdate(ARFaceUpdatedEventArgs eventArgs)
        {
            Result<NativeArray<XRFaceBlendShape>> result =
                FaceManager.TryGetBlendShapes(eventArgs.face, Allocator.Temp);
            if (result.status.IsSuccess())
            {
                for (int i = 0;
                     i < result.value.Length && i < _avatarFaceData.Parameters.Length;
                     i++)
                {
                    _avatarFaceData.Parameters[i] = result.value[i].weight;
                }
            }
        }

        private void OnDestroy()
        {
            FaceManager.trackablesChanged.RemoveListener(OnFaceChanged);
        }
    }
}
