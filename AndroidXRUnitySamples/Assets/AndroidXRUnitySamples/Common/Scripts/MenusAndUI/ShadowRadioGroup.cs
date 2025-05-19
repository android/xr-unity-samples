// <copyright file="ShadowRadioGroup.cs" company="Google LLC">
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A manager class for facilitating communication between ShadowRadioButtons.
    /// </summary>
    public class ShadowRadioGroup : MonoBehaviour
    {
        [SerializeField]
        private ShadowRadioButton _selectedButton;

        private List<ShadowRadioButton> _buttons;

        private Dictionary<Key, ShadowRadioButton> _shortcutToButtonMap =
            new Dictionary<Key, ShadowRadioButton>();

        /// <summary>
        /// Function called by ShadowRadioButtons to register with this group.
        /// </summary>
        /// <param name="button">ShadowRadioButton to register.</param>
        public void RegisterButton(ShadowRadioButton button)
        {
            _buttons.Add(button);

            if (button.KeyboardShortcut != Key.None)
            {
                if (_shortcutToButtonMap.ContainsKey(button.KeyboardShortcut))
                {
                    Debug.LogError(
                        $"Duplicate shortcut: {button.KeyboardShortcut} from {button.name}");
                }
                else
                {
                    _shortcutToButtonMap.Add(button.KeyboardShortcut, button);
                }
            }
        }

        /// <summary>Function called by ShadowRadioButtons to know if they're selected.</summary>
        /// <param name="button">ShadowRadioButton to check against.</param>
        /// <returns>If button is the selected radio button.</returns>
        public bool AmITheOne(ShadowRadioButton button)
        {
            return _selectedButton == button;
        }

        /// <summary>Function called by ShadowRadioButtons to flag as selected.</summary>
        /// <param name="button">ShadowRadioButton to set as selected.</param>
        public void SetSelectedButton(ShadowRadioButton button)
        {
            _selectedButton = button;
        }

        void Awake()
        {
            _buttons = new List<ShadowRadioButton>();
        }

        void Update()
        {
            foreach (KeyValuePair<Key, ShadowRadioButton> entry in _shortcutToButtonMap)
            {
                if (Keyboard.current[entry.Key].wasPressedThisFrame)
                {
                    entry.Value.TriggerPress();
                }
            }
        }
    }
}
