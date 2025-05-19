// <copyright file="DebugModeActivator.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// A behaviour that activates and deactivates game objects based on debug mode setting.
    /// </summary>
    public class DebugModeActivator : MonoBehaviour
    {
        /// <summary>
        /// Reference to the debug mode setting.
        /// </summary>
        public BoolVariable DebugModeSetting;

        /// <summary>
        /// Objects that will be enabled in debug mode, and disabled otherwise.
        /// </summary>
        public GameObject[] EnableInDebugMode;

        /// <summary>
        /// Objects that will be disabled in debug mode, and enabled otherwise.
        /// </summary>
        public GameObject[] DisableInDebugMode;

        void OnEnable()
        {
            DebugModeSetting.AddListener(HandleDebugModeChange);
            HandleDebugModeChange(DebugModeSetting.Value);
        }

        void OnDisable()
        {
            DebugModeSetting.RemoveListener(HandleDebugModeChange);
        }

        void HandleDebugModeChange(bool debugMode)
        {
            foreach (GameObject g in EnableInDebugMode)
            {
                g.SetActive(debugMode);
            }

            foreach (GameObject g in DisableInDebugMode)
            {
                g.SetActive(!debugMode);
            }
        }
    }
}
