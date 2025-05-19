// <copyright file="FlashlightControls.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Script for controlling the flashlight.
    /// </summary>
    public class FlashlightControls : MonoBehaviour
    {
        [SerializeField] private Transform _flashlightBase;
        [SerializeField] private InputActionProperty _leftHandAction;
        [SerializeField] private InputActionProperty _rightHandAction;
        [SerializeField] private InputActionReference _leftHandPositionAxis;
        [SerializeField] private InputActionReference _leftHandRotationAxis;
        [SerializeField] private InputActionReference _rightHandPositionAxis;
        [SerializeField] private InputActionReference _rightHandRotationAxis;

        private bool _enabled;
        private bool _attachedToLeft;

        /// <summary>
        /// Returns true if the flashlight is currently lit.
        /// </summary>
        public bool Enabled => _flashlightBase.gameObject.activeSelf;

        private void Start()
        {
            // Start disabled.
            _flashlightBase.gameObject.SetActive(false);
            _enabled = false;
        }

        private void Update()
        {
            if (_enabled)
            {
                if ((_attachedToLeft && !_leftHandAction.action.IsPressed()) ||
                    (!_attachedToLeft && !_rightHandAction.action.IsPressed()))
                {
                    DetachFromAxes();
                    _flashlightBase.gameObject.SetActive(false);
                    _enabled = false;
                }
            }
            else
            {
                if (_leftHandAction.action.IsPressed())
                {
                    _flashlightBase.gameObject.SetActive(true);
                    AttachToAxes(left: true);
                    _enabled = true;
                }
                else if (_rightHandAction.action.IsPressed())
                {
                    _flashlightBase.gameObject.SetActive(true);
                    AttachToAxes(left: false);
                    _enabled = true;
                }
            }
        }

        private void AttachToAxes(bool left)
        {
            _attachedToLeft = left;

            if (left)
            {
                ((InputAction)_leftHandPositionAxis).performed += OnTrackedPositionAxisUpdate;
                ((InputAction)_leftHandRotationAxis).performed += OnTrackedRotationAxisUpdate;
            }
            else
            {
                ((InputAction)_rightHandPositionAxis).performed += OnTrackedPositionAxisUpdate;
                ((InputAction)_rightHandRotationAxis).performed += OnTrackedRotationAxisUpdate;
            }
        }

        private void DetachFromAxes()
        {
            if (_attachedToLeft)
            {
                ((InputAction)_leftHandPositionAxis).performed -= OnTrackedPositionAxisUpdate;
                ((InputAction)_leftHandRotationAxis).performed -= OnTrackedRotationAxisUpdate;
            }
            else
            {
                ((InputAction)_rightHandPositionAxis).performed -= OnTrackedPositionAxisUpdate;
                ((InputAction)_rightHandRotationAxis).performed -= OnTrackedRotationAxisUpdate;
            }
        }

        private void OnTrackedPositionAxisUpdate(InputAction.CallbackContext ctx)
        {
            transform.position = ctx.ReadValue<Vector3>();
        }

        private void OnTrackedRotationAxisUpdate(InputAction.CallbackContext ctx)
        {
            transform.rotation = ctx.ReadValue<Quaternion>();
        }
    }
}
