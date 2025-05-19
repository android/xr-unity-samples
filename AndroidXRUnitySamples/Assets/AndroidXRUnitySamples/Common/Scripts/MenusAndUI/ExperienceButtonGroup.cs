// <copyright file="ExperienceButtonGroup.cs" company="Google LLC">
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
    /// Behaviour for the experience button grouping logic on the Menus.
    /// </summary>
    public class ExperienceButtonGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Image _background;
        [SerializeField] private Vector2 _buttonPosAnchor;
        [SerializeField] private Vector2 _buttonPosSpacing;
        [SerializeField] private float _minHeight;

        private int _buttonCount;

        /// <summary>
        /// Set the color of the group.
        /// </summary>
        /// <param name="col">The color of the group.</param>
        public void SetColor(Color col)
        {
            _background.color = col;
        }

        /// <summary>
        /// Set the title of the group.
        /// </summary>
        /// <param name="title">The title of the group.</param>
        public void SetTitleText(string title)
        {
            _titleText.text = title;
        }

        /// <summary>
        /// Add a button in the correct spot and update our bounds to match.
        /// </summary>
        /// <param name="button">The button to parent to us.</param>
        public void AddChildButton(RectTransform button)
        {
            button.SetParent(transform, false);

            Vector3 localPos = Vector3.zero;
            localPos.x = _buttonPosAnchor.x +
                ((_buttonCount % 2) * _buttonPosSpacing.x);
            localPos.y = _buttonPosAnchor.y -
                ((_buttonCount / 2) * _buttonPosSpacing.y);
            button.anchoredPosition = localPos;
            button.localRotation = Quaternion.identity;

            RectTransform rt = GetComponent<RectTransform>();
            Vector2 sizeDelta = rt.sizeDelta;
            sizeDelta.y = _minHeight + _buttonPosSpacing.y * (_buttonCount / 2);
            rt.sizeDelta = sizeDelta;

            ++_buttonCount;
        }

        private void Awake()
        {
            _buttonCount = 0;
        }
    }
}
