// <copyright file="AppFocusManager.cs" company="Google LLC">
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>Handles app (de)focus actions.</summary>
    public class AppFocusManager : MonoBehaviour
    {
        /// <summary>List of gameobjects to toggle.</summary>
        public List<GameObject> ObjectsToControl;

        /// <summary>Action for focus gained.</summary>
        public static event Action OnFocusGainedAction;

        /// <summary>Action for focus lost.</summary>
        public static event Action OnFocusLostAction;

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetObjectsActive(true);
                OnFocusGainedAction?.Invoke();
            }
            else
            {
                SetObjectsActive(false);
                OnFocusLostAction?.Invoke();
            }
        }

        void SetObjectsActive(bool active)
        {
            foreach (GameObject obj in ObjectsToControl)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
    }
}
