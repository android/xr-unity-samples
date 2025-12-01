// <copyright file="DeactivateBasedOn.cs" company="Google LLC">
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
    /// Controls the active state of a GameObject based on specified conditions
    /// such as scene start, editor mode, or build mode.
    /// </summary>
    public class DeactivateBasedOn : MonoBehaviour
    {
        [Tooltip("If true, the GameObject will be deactivated when the scene starts.")]
        [SerializeField] private bool _deactivateOnStart = false;

        [Tooltip("If true, the GameObject will be deactivated when running in the Unity editor.")]
        [SerializeField] private bool _deactivateInEditor = false;

        [Tooltip("If true, the GameObject will be deactivated when running in a standalone build.")]
        [SerializeField] private bool _deactivateInBuild = false;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Deactivates the GameObject based on the inspector settings.
        /// </summary>
        void Start()
        {
            bool shouldDeactivate = false;

            if (_deactivateOnStart)
            {
                shouldDeactivate = true;
            }

            if (_deactivateInEditor)
            {
#if UNITY_EDITOR
                shouldDeactivate = true;
#endif
            }

            if (_deactivateInBuild)
            {
#if !UNITY_EDITOR
                shouldDeactivate = true;
#endif
            }

            if (shouldDeactivate)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
