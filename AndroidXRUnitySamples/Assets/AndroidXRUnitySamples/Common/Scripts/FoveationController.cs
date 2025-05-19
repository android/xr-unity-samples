// <copyright file="FoveationController.cs" company="Google LLC">
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Component used to change passthrough blend mode at run time.
    /// </summary>
    public class FoveationController : MonoBehaviour
    {
        private float _currentFoveationLevel = 0f;
        private List<XRDisplaySubsystem> _xrDisplays = new List<XRDisplaySubsystem>();

        /// <summary>
        /// Gets or sets a value indicating the foveation level to use.
        /// </summary>
        public float FoveationLevel
        {
            get
            {
                return _currentFoveationLevel;
            }

            set
            {
                if (value < 0f || value > 1f)
                {
                    Debug.LogError($"Invalid foveation level: {value}");
                    return;
                }

#if !UNITY_EDITOR
                SubsystemManager.GetSubsystems(_xrDisplays);
                if (_xrDisplays.Count != 1)
                {
                    Debug.LogError($"Invalid display subsystem: {_xrDisplays.Count}");
                    return;
                }

                _xrDisplays[0].foveatedRenderingLevel = value;
#endif
                _currentFoveationLevel = value;
                Debug.Log($"Foveation level set: {_currentFoveationLevel}");
            }
        }
    }
}
