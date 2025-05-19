// <copyright file="ExperienceButton.cs" company="Google LLC">
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
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Behaviour for the Experience Button UI component.
    /// </summary>
    public class ExperienceButton : MonoBehaviour
    {
        [SerializeField]
        private ShadowButton _shadowButton;
        [SerializeField]
        private GameObject _inputModeErrorOverlay;
        [SerializeField]
        private TMP_Text _inputModeErrorOverlayCaption;
        [SerializeField]
        private GameObject _disabledOverlay;

        private ExperienceSettings _experience;
        private MenuManager _parentMenu;

        /// <summary>
        /// Initialize the button for this experience.
        /// </summary>
        /// <param name="parent">The menumanager owner of this button.</param>
        /// <param name="experience">The settings for the experience.</param>
        public void Init(MenuManager parent, ExperienceSettings experience)
        {
            _parentMenu = parent;
            _experience = experience;

            if (_shadowButton is null)
            {
                _shadowButton = GetComponent<ShadowButton>();
            }

            _shadowButton.SetButtonText(experience.ExperienceName);
            _shadowButton.SetButtonIcon(experience.Icon);

            List<String> supportedModes = _experience.SupportedInputModes.ConvertAll(
                new Converter<XRInputModalityManager.InputMode, String>(InputModeToString));
            String supportedModesStr = String.Join(" or ", supportedModes);
            _inputModeErrorOverlayCaption.text = $"Requires {supportedModesStr}";

            DisableButton(experience.ForceDisable, false);
        }

        /// <summary>
        /// Behaviour of an Experience Button when that button is clicked.
        /// </summary>
        public void OnButtonClick()
        {
            _parentMenu.ExperienceButtonClicked(_experience);
        }

        /// <summary>
        /// Handles the input mode change by enabling/disabling this button based on the
        /// experience's supported input modes.
        /// </summary>
        /// <param name="inputMode">The new input mode.</param>
        public void HandleInputModeChange(XRInputModalityManager.InputMode inputMode)
        {
            DisableButton(_experience.ForceDisable,
                !_experience.SupportedInputModes.Contains(inputMode));
        }

        private void DisableButton(bool forceDisable, bool inputError)
        {
            _shadowButton.SetButtonDisabled(forceDisable || inputError);
            _disabledOverlay.SetActive(forceDisable);
            _inputModeErrorOverlay.SetActive(!forceDisable && inputError);
        }

        private String InputModeToString(XRInputModalityManager.InputMode mode)
        {
            switch (mode)
            {
                case XRInputModalityManager.InputMode.None:
                    return "None";
                case XRInputModalityManager.InputMode.TrackedHand:
                    return "Hands";
                case XRInputModalityManager.InputMode.MotionController:
                    return "Controllers";
            }

            return "Unknown";
        }
    }
}
