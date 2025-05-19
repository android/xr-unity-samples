// <copyright file="SettingsControls.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Exposes and applies controls for the XRUI scene input modes and passthrough.
    /// </summary>
    public class SettingsControls : MonoBehaviour
    {
        [SerializeField]
        private BoolVariable _passthroughEnabled;

        [SerializeField]
        private BoolVariable _debugModeSetting;

        [SerializeField]
        private FloatVariable _passthroughLevel;

        [SerializeField]
        private InputMode _inputMode;

        [SerializeField]
        private XRGazeInteractor _eyesGazeInteractor;

        [SerializeField]
        private InputActionProperty _pinchInputAction;

        [SerializeField]
        private Timer _eyeReticleAnimationControlTimer;

        [SerializeField]
        private BoolVariable _appMenuIsOpen;

        private bool _initialized;
        private bool _passthroughInitialState;
        private InputMode.XruiInputMode _lastInputMode;

        /// <summary>
        /// Handles the change of input mode.
        /// </summary>
        /// <param name="selection">Which input mode was last selected.</param>
        public void HandleInputModeSelectionChanged(Toggle selection)
        {
            if (Enum.TryParse(selection?.name, out InputMode.XruiInputMode inputMode))
            {
                _inputMode.Value = inputMode;
            }
        }

        private static void EnablePassthrough(bool ptEnabled)
        {
            Singleton.Instance.OriginManager.EnablePassthrough = ptEnabled;
        }

        private void LogInputDevices()
        {
            if (!_debugModeSetting.Value)
            {
                return;
            }

            var xrDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(xrDevices);
            ReadOnlyArray<InputDevice> devices = InputSystem.devices;

            Debug.Log("::::: UnityEngine.InputSystem.devices :::::\n" +
                      $"{devices.Count} connected InputSystem input devices:\n");
            foreach (InputDevice d in devices)
            {
                string str = d.deviceId + " - " + d.name + " - enabled: " + d.enabled
                           + " - native:" + d.native;
                Debug.Log(
                        $"{str}");
            }

            Debug.Log("::::: UnityEngine.XR.InputDevices :::::\n" +
                      $"{xrDevices.Count} connected XR input devices:\n");

            foreach (UnityEngine.XR.InputDevice d in xrDevices)
            {
                string str = d.name + " - isValid: " + d.isValid + " - subsystem:" + d.subsystem;
                Debug.Log(
                        $"{str}");
            }
        }

        private void TogglePassthrough(bool toggleValue)
        {
            EnablePassthrough(toggleValue);
        }

        private void SetInputModeEyeAndHands()
        {
            _eyesGazeInteractor.uiPressInput.inputActionPerformed.Enable();
            _eyesGazeInteractor.selectInput.inputActionPerformed.Enable();
            _eyesGazeInteractor.hoverToSelect = false;
            _eyeReticleAnimationControlTimer.enabled = false;
            Singleton.Instance.OriginManager.EnableGazeInteraction = true;
            Singleton.Instance.OriginManager.EnableHandRaysInteraction = false;
        }

        private void SetInputModeHandsOnly()
        {
            Singleton.Instance.OriginManager.EnableGazeInteraction = false;
            Singleton.Instance.OriginManager.EnableHandRaysInteraction = true;
        }

        private void SetInputModeEyesOnly()
        {
            _eyesGazeInteractor.uiPressInput.inputActionPerformed.Disable();
            _eyesGazeInteractor.selectInput.inputActionPerformed.Disable();
            _eyesGazeInteractor.hoverToSelect = true;
            _eyeReticleAnimationControlTimer.enabled = true;
            Singleton.Instance.OriginManager.EnableGazeInteraction = true;
            Singleton.Instance.OriginManager.EnableHandRaysInteraction = false;
        }

        private void SetInputMode(InputMode.XruiInputMode inputMode)
        {
            switch (inputMode)
            {
                case InputMode.XruiInputMode.EyesAndHands:
                    SetInputModeEyeAndHands();
                    break;
                case InputMode.XruiInputMode.HandsOnly:
                    SetInputModeHandsOnly();
                    break;
                case InputMode.XruiInputMode.EyesOnly:
                    SetInputModeEyesOnly();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputMode), inputMode, null);
            }
        }

        private void SwitchInputModesForAppMenu(bool menuOpen)
        {
            if (menuOpen)
            {
                _lastInputMode = _inputMode.Value;
                _inputMode.Value = InputMode.XruiInputMode.HandsOnly;
            }
            else
            {
                _inputMode.Value = _lastInputMode;
            }
        }

        private void Start()
        {
            _passthroughInitialState = _passthroughEnabled.Value;
            _lastInputMode = InputMode.XruiInputMode.HandsOnly;
            _eyesGazeInteractor.uiPressInput.inputActionPerformed = _pinchInputAction.action;
            _eyesGazeInteractor.selectInput.inputActionPerformed = _pinchInputAction.action;
        }

        private void Update()
        {
            if (!_initialized)
            {
                _passthroughEnabled.Value = true;
                _passthroughLevel.Value = 1f;
                TogglePassthrough(_passthroughEnabled.Value);
                SetInputMode(_inputMode.Value);

                LogInputDevices();

                _initialized = true;
            }
        }

        private void OnEnable()
        {
            _passthroughEnabled.AddListener(TogglePassthrough);
            _inputMode.AddListener(SetInputMode);
            _appMenuIsOpen.AddListener(SwitchInputModesForAppMenu);
        }

        private void OnDisable()
        {
            _passthroughEnabled.RemoveListener(TogglePassthrough);
            _inputMode.RemoveListener(SetInputMode);
            _appMenuIsOpen.RemoveListener(SwitchInputModesForAppMenu);

            _initialized = false;
        }

        private void OnDestroy()
        {
            _passthroughEnabled.RemoveListener(TogglePassthrough);
            _inputMode.RemoveListener(SetInputMode);

            SetInputModeHandsOnly();
            EnablePassthrough(_passthroughInitialState);
        }
    }
}
