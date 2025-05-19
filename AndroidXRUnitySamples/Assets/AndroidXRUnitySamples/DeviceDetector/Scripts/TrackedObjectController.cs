// <copyright file="TrackedObjectController.cs" company="Google LLC">
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

using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples.DeviceDetector
{
    /// <summary>
    /// Class used to manage a tracked object.
    /// </summary>
    public class TrackedObjectController : MonoBehaviour
    {
        [SerializeField] private SceneState _sceneState;
        [SerializeField] private ARTrackedObject _arTrackedObject;
        [SerializeField] private GameObject _debugMesh;

        private bool _isKeyboard;

        private void Start()
        {
            bool isKeyboard = false;
#if XR_FEATURE_OBJECT_TRACKING
            isKeyboard = _arTrackedObject.GetObjectLabel() == XRObjectLabel.Keyboard;
#endif // XR_FEATURE_OBJECT_TRACKING

            if (isKeyboard)
            {
                _isKeyboard = true;
            }
            else
            {
                _isKeyboard = false;
                _debugMesh.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_isKeyboard)
            {
                _sceneState.KeyboardTracked.Value = false;
            }
        }

        private void Update()
        {
            if (_isKeyboard)
            {
                _sceneState.KeyboardTracked.Value = true;
                _sceneState.KeyboardPos.Value = transform.position;
                _sceneState.KeyboardRot.Value = transform.rotation;
#if XR_FEATURE_OBJECT_TRACKING
                _sceneState.KeyboardExtents.Value = _arTrackedObject.GetExtents();
                _debugMesh.transform.localScale = _arTrackedObject.GetExtents();
#endif // XR_FEATURE_OBJECT_TRACKING
            }
        }
    }
}
