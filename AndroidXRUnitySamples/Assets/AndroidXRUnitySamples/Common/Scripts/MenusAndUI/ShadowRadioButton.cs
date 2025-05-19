// <copyright file="ShadowRadioButton.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A shadow radio button.
    /// </summary>
    public class ShadowRadioButton : ShadowButton
    {
        [SerializeField]
        private Key _keyboardShortcut = Key.None;

        [SerializeField]
        private ShadowRadioGroup _radioGroup;

        /// <summary>
        /// Gets the keyboard shortcut to trigger press of the button.
        /// Keyboard shortcut code `KeyCode.None` means the button has no shortcut.
        /// </summary>
        public Key KeyboardShortcut
        {
            get
            {
                return _keyboardShortcut;
            }

            private set
            {
                _keyboardShortcut = value;
            }
        }

        /// <summary>Overridable function for accessing pressed state.</summary>
        /// <returns>If button is pressed or the selected radio button.</returns>
        protected override bool ButtonIsPressed()
        {
            return base.ButtonIsPressed() || _radioGroup.AmITheOne(this);
        }

        /// <summary>Overridable function for triggering hover sound.</summary>
        protected override void PlayHoverSound()
        {
            // Don't play the sound if we're already pressed.
            if (!_radioGroup.AmITheOne(this))
            {
                base.PlayHoverSound();
            }
        }

        void Start()
        {
            _radioGroup.RegisterButton(this);
            OnPress.AddListener(delegate { _radioGroup.SetSelectedButton(this); });
        }
    }
}
