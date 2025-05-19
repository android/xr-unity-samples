// <copyright file="StatusItem.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Home
{
    /// <summary>
    /// Controller for an item on the dashboard in Home.
    /// </summary>
    public class StatusItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Image _image;
        [SerializeField] private Image _bgImage;
        [SerializeField] private Sprite _validIcon;
        [SerializeField] private Sprite _invalidIcon;
        [SerializeField] private Color _goodStatusColor;
        [SerializeField] private Color _badStatusColor;

        /// <summary>
        /// Function for updating text and image icon.
        /// </summary>
        /// <param name="text">Text for _valueText.</param>
        /// <param name="good">Flag for using _validIcon vs _invalidIcon.</param>
        public void UpdateStatus(string text, bool good)
        {
            _valueText.text = text;
            _image.sprite = good ? _validIcon : _invalidIcon;
            _image.enabled = true;
            _valueText.color = good ? _goodStatusColor : _badStatusColor;
            _image.color = good ? _goodStatusColor : _badStatusColor;
            _bgImage.color = good ? _goodStatusColor : _badStatusColor;
        }
    }
}
