// <copyright file="AvatarModelSwitcher.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Manages the visibility and switching of different avatar models in the scene.
    /// </summary>
    public class AvatarModelSwitcher : MonoBehaviour
    {
        [Tooltip("Whether to hide all the avatar models.")]
        [SerializeField] private bool _hideAll;

        [Tooltip("Array of GameObject models representing different avatars.")]
        [SerializeField] private GameObject[] _avatarModels;

        [Tooltip("Array of GameObjects representing the avatar meshes.")]
        [SerializeField] private GameObject[] _avatarMeshes;

        [Tooltip("The index of the currently active avatar model.")]
        [SerializeField] private int _currentAvatarIndex = 4;

        [SerializeField] private float _avatarSwitchDelay = 0.15f;

        /// <summary>
        /// The total number of avatar models available in the <see cref="_avatarModels"/> array.
        /// </summary>
        private int _nAvatars;

        /// <summary>
        /// Switches to the next avatar model in the array.
        /// Wraps around to the beginning if at the end of the array.
        /// </summary>
        public void NextAvatar()
        {
            _currentAvatarIndex = (_currentAvatarIndex + 1) % _nAvatars;
            UpdateAvatarVisibility();
        }

        /// <summary>
        /// Switches to the previous avatar model in the array.
        /// Wraps around to the end if at the beginning of the array.
        /// </summary>
        public void PreviousAvatar()
        {
            // Calculate previous index, handling wrap-around for negative results
            _currentAvatarIndex = (_currentAvatarIndex - 1 + _nAvatars) % _nAvatars;
            UpdateAvatarVisibility();
        }

        private void Awake()
        {
            _nAvatars = _avatarModels.Length;
            InitializeAvatars();
        }

        private void Start()
        {
            UpdateAvatarVisibility();
        }

        /// <summary>
        /// Updates the visibility of the avatar models, showing only the current one.
        /// All other avatar models in the <see cref="_avatarModels"/> array will be set to
        /// inactive.
        /// </summary>
        private void UpdateAvatarVisibility()
        {
            for (int i = 0; i < _nAvatars; i++)
            {
                _avatarModels[i].SetActive(false);
            }

            if (_hideAll)
            {
                return;
            }

            // Ensure there's at least one avatar to prevent index out of bounds if _nAvatars is 0
            if (_nAvatars > 0)
            {
                StartCoroutine(DelayActivateAvatar(_currentAvatarIndex));
            }
            else
            {
                Debug.LogWarning("No avatar models assigned to AvatarModelSwitcher.");
            }
        }

        /// <summary>
        /// Initializes all the avatars models to be ready for the scene.
        /// </summary>
        private void InitializeAvatars()
        {
            for (int i = 0; i < _nAvatars; i++)
            {
                _avatarModels[i].SetActive(true);
            }
        }

        private IEnumerator DelayActivateAvatar(int index)
        {
            // Enable the avatar but delay the mesh rendering.
            // The tracking takes a frame or two to kick in, so this mesh hiding prevents an
            // animation pop from the t-pose to tracking pose.
            _avatarModels[index].SetActive(true);
            _avatarMeshes[index].SetActive(false);
            yield return new WaitForSeconds(_avatarSwitchDelay);
            _avatarMeshes[index].SetActive(true);
        }
    }
}
