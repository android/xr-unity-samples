// <copyright file="GeminiMaterialsDebug.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Class used to show debug info for Gemini Materials when in debug mode.
    /// </summary>
    public class GeminiMaterialsDebug : MonoBehaviour
    {
        [SerializeField] private BoolVariable _debugMode;
        [SerializeField] private GameObject _debugContainer;
        [SerializeField] private Vector3 _positionOffset;

        private void OnEnable()
        {
            _debugMode.AddListener(HandleDebugModeChange);
            HandleDebugModeChange(_debugMode.Value);
        }

        private void OnDisable()
        {
            _debugMode.RemoveListener(HandleDebugModeChange);
        }

        private void HandleDebugModeChange(bool debug)
        {
            _debugContainer.SetActive(debug);

            // If we're just set active, reposition in the scene in front of the user.
            if (debug)
            {
                Transform camXf = Singleton.Instance.Camera.transform;
                Vector3 fwd = camXf.forward;
                fwd.y = 0.0f;
                Vector3 newPos = camXf.position + fwd.normalized * _positionOffset.z;
                newPos.y -= _positionOffset.y;
                transform.position = newPos;
                transform.forward = (transform.position - camXf.position).normalized;
            }
        }
    }
}
