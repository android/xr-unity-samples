// <copyright file="BlendModeController.cs" company="Google LLC">
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
using Google.XR.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Component used to change passthrough blend mode at run time.
    /// </summary>
    public class BlendModeController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the blend mode feature.
        /// </summary>
        public XREnvironmentBlendModeFeature BlendFeature;

        /// <summary>
        /// Sets the blend mode.
        /// </summary>
        /// <param name="mode">Blend mode to use.</param>
        public void SetBlendMode(XrEnvironmentBlendMode mode)
        {
#if !UNITY_EDITOR
            if (BlendFeature == null)
            {
                Debug.LogWarning("Trying to set blend mode when blend feature is null");
                return;
            }

            StartCoroutine(DelayedSetBlendMode(mode));
#endif
            Debug.Log($"Blend mode set: {mode}");
        }

        IEnumerator DelayedSetBlendMode(XrEnvironmentBlendMode mode)
        {
            yield return new WaitForSeconds(0.1f);
            BlendFeature.RequestedEnvironmentBlendMode = mode;
        }
    }
}
