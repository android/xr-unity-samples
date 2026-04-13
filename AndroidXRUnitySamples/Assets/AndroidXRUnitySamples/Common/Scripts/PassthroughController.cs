// <copyright file="PassthroughController.cs" company="Google LLC">
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
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Component used to change passthrough blend mode at run time.
    /// </summary>
    public class PassthroughController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the AR Camera Manager.
        /// </summary>
        public ARCameraManager ARCameraManager;

        /// <summary>
        /// Sets passthrough to enabled or disabled.
        /// </summary>
        /// <param name="enabled">Whether to enable passthrough.</param>
        public void SetPassthrough(bool enabled)
        {
#if !UNITY_EDITOR
            if (ARCameraManager == null)
            {
                Debug.LogWarning("Trying to set blend mode when ARCameraManager is null");
                return;
            }

            StartCoroutine(DelayedSetBlendMode(enabled));
#endif
            Debug.Log($"Passthrough set: {enabled}");
        }

        IEnumerator DelayedSetBlendMode(bool enabled)
        {
            yield return new WaitForSeconds(0.1f);
            ARCameraManager.enabled = enabled;
        }

        private void Start()
        {
            SetPassthrough(true);
            Singleton.Instance.Camera.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}
