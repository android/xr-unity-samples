// <copyright file="ObjectPlacementUI.cs" company="Google LLC">
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
using AndroidXRUnitySamples.MenusAndUI;
using UnityEngine;

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// UI for object placement.
    /// </summary>
    public class ObjectPlacementUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the scene state data.
        /// </summary>
        public SceneState SceneState;

        /// <summary>
        /// References to the object spawn UI buttons.
        /// </summary>
        public UIButton[] UIButtons;

        private void HandleObjectSpawn(SpawnNewObjectEvent.ObjectType type, Vector3 origin)
        {
            var info = new SpawnNewObjectEvent.Info();
            info.Type = type;
            info.Origin = origin;
            SceneState.OnSpawnNewObject.Invoke(info);
        }

        private void Start()
        {
            foreach (UIButton uiButton in UIButtons)
            {
                uiButton.Button.OnPress.AddListener(
                        () =>
                        {
                            HandleObjectSpawn(uiButton.Type, uiButton.Button.transform.position);
                        });
            }
        }

        /// <summary>
        /// UI button data.
        /// </summary>
        [Serializable]
        public class UIButton
        {
            /// <summary>
            /// Reference to the button.
            /// </summary>
            public ShadowButton Button;

            /// <summary>
            /// Type of spawned object.
            /// </summary>
            public SpawnNewObjectEvent.ObjectType Type;
        }
    }
}
