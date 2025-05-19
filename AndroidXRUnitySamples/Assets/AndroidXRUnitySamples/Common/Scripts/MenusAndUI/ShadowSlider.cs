// <copyright file="ShadowSlider.cs" company="Google LLC">
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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A shadow slider.
    /// </summary>
    public class ShadowSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] TMP_Text _valueLabel;
        [SerializeField] private string[] _values;

        void Awake()
        {
            if (_values.Length < 2)
            {
                Debug.LogError("Not enough values set for the slider", this);
            }

            _slider.minValue = 0;
            _slider.maxValue = _values.Length - 1;

            _slider.onValueChanged.AddListener(RefreshVisuals);
            _slider.onValueChanged.AddListener(PlayAudio);
            RefreshVisuals(_slider.value);
        }

        void RefreshVisuals(float value)
        {
            int index = (int)value;
            _valueLabel.text = _values[index];
        }

        void PlayAudio(float value)
        {
            // Don't play the sound if we're not enabled.
            // This prevents sounds from being played during instantiation.
            if (gameObject.activeInHierarchy)
            {
                Singleton.Instance.Audio.PlaySliderValueChange(transform.position);
            }
        }
     }
}
