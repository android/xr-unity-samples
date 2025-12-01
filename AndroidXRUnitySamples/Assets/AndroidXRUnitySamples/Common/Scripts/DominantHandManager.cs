// <copyright file="DominantHandManager.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Continuously checks the Meta aim flags to identify the dominant hand and updates
    /// the variable when the dominant hand is identified.
    /// </summary>
    public class DominantHandManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference _leftHandAimFlagsInputAction;
        [SerializeField] private InputActionReference _rightHandAimFlagsInputAction;
        [SerializeField] private BoolVariable _leftDominantHandVariable;
        [SerializeField] private InputActionReference _leftHandApplicationMenuInputAction;
        [SerializeField] private InputActionReference _rightHandApplicationMenuInputAction;
        [SerializeField] private InputActionReference _leftHandPinchPositionInputAction;
        [SerializeField] private InputActionReference _rightHandPinchPositionInputAction;

        /// <summary>
        /// Checks whether the menu button was pressed this frame.
        /// </summary>
        /// <returns>Whether the menu button was pressed this frame.</returns>
        public bool GetMenuButtonPressedThisFrame()
        {
            return _leftDominantHandVariable.Value ?
                   _rightHandApplicationMenuInputAction.action.WasPressedThisFrame() :
                   _leftHandApplicationMenuInputAction.action.WasPressedThisFrame();
        }

        /// <summary>
        /// Returns the location of the pinch position on the nondominant hand.
        /// </summary>
        /// <returns>Pinch position of the nondominant hand.</returns>
        public Vector3 GetNondominantPinchPosition()
        {
            return _leftDominantHandVariable.Value ?
                   _rightHandPinchPositionInputAction.action.ReadValue<Vector3>() :
                   _leftHandPinchPositionInputAction.action.ReadValue<Vector3>();
        }

        private void Update()
        {
            if (!_leftDominantHandVariable.Value &&
                (_leftHandAimFlagsInputAction.action.ReadValue<int>() &
                 (int)MetaAimFlags.DominantHand) != 0)
            {
                _leftDominantHandVariable.Value = true;
            }
            else if (_leftDominantHandVariable.Value &&
                (_rightHandAimFlagsInputAction.action.ReadValue<int>() &
                 (int)MetaAimFlags.DominantHand) != 0)
            {
                _leftDominantHandVariable.Value = false;
            }
        }
    }
}
