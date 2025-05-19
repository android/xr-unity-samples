// <copyright file="Singleton.cs" company="Google LLC">
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

using AndroidXRUnitySamples.MenusAndUI;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Singleton pattern with AndroidXRUnitySamples specific functionality.
    /// </summary>
    public class Singleton : MonoBehaviour
    {
        /// <summary>
        /// Should this object persist across scene changes.
        /// </summary>
        public bool PersistSceneChange;

        /// <summary>
        /// The associated <see cref="AndroidXROriginManager" />.
        /// </summary>
        public AndroidXROriginManager OriginManager;

        /// <summary>
        /// The associated <see cref="SceneManager" />.
        /// </summary>
        public SceneManager SceneManager;

        /// <summary>
        /// Container for the settings.
        /// </summary>
        public Settings Settings;

        /// <summary>
        /// The <see cref="XrOrigin" /> rig object in the scene.
        /// </summary>
        public XROrigin XROrigin;

        /// <summary>
        /// The main camera in the scene.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// The audio manager in the scene.
        /// </summary>
        public AudioManager Audio;

        /// <summary>
        /// The Main Menu.
        /// </summary>
        public MenuManager Menu;

        /// <summary>
        /// Gets the single instance of this behaviour type.
        /// </summary>
        public static Singleton Instance
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (PersistSceneChange)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
