// <copyright file="ShadowToggle.cs" company="Google LLC">
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
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A shadow toggle.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ShadowToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggleSibling;
        [SerializeField] private GameObject _nobImageOn;
        [SerializeField] private GameObject _nobImageOff;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _backgroundOff;
        [SerializeField] private Color _backgroundOn;
        [SerializeField] private GameObject _labelOn;
        [SerializeField] private GameObject _labelOff;

        /// <summary>Update the visual representation of this toggle.</summary>
        /// <param name="isOn">Is the toggle enabled.</param>
        public void RefreshVisuals(bool isOn)
        {
            if (isOn)
            {
                _nobImageOn.SetActive(true);
                _nobImageOff.SetActive(false);
                _backgroundImage.color = _backgroundOn;
                if (_labelOn)
                {
                    _labelOn.SetActive(true);
                }

                if (_labelOff)
                {
                    _labelOff.SetActive(false);
                }
            }
            else
            {
                _nobImageOn.SetActive(false);
                _nobImageOff.SetActive(true);
                _backgroundImage.color = _backgroundOff;
                if (_labelOn)
                {
                    _labelOn.SetActive(false);
                }

                if (_labelOff)
                {
                    _labelOff.SetActive(true);
                }
            }
        }

        void Awake()
        {
            _toggleSibling.onValueChanged.AddListener(RefreshVisuals);
            _toggleSibling.onValueChanged.AddListener(PlayAudio);
            RefreshVisuals(_toggleSibling.isOn);
        }

        void PlayAudio(bool isOn)
        {
            // Don't play the sound if we're not enabled.
            // This prevents sounds from being played during instantiation.
            if (gameObject.activeInHierarchy)
            {
                Singleton.Instance.Audio.PlayTogglePress(transform.position);
            }
        }
    }
}
