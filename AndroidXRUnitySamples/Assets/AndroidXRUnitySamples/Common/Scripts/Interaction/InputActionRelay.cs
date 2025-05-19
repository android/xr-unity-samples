// <copyright file="InputActionRelay.cs" company="Google LLC">
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
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Attaches UnityEvents to an input action. Listens to input action events (started, performed,
    /// canceled) and invokes corresponding Unity events when the input actions are invoked. Makes
    /// it easier to integrate Input System actions with other scene objects in an editor-friendly
    /// way.
    /// <see
    ///     href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputAction.html">
    /// InputAction
    /// Class
    /// </see>
    /// </summary>
    public class InputActionRelay : MonoBehaviour
    {
        /// <summary>
        /// The input action to listen for events.
        /// <see
        ///     href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputActionProperty.html">
        /// InputAction
        /// Class
        /// </see>
        /// </summary>
        [Tooltip("The input action to listen for events.")]
        public InputActionProperty InputAction;

        /// <summary>
        /// Triggered when the input action starts.
        /// <see
        ///     href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_started">
        /// InputAction.started
        /// Event
        /// </see>
        /// </summary>
        [Tooltip("Triggered when the input action starts.")]
        public UnityEvent<InputAction.CallbackContext> InputActionStarted;

        /// <summary>
        /// Triggered when the input action is performed.
        /// <see
        ///     href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_performed">
        /// InputAction.performed
        /// Event
        /// </see>
        /// </summary>
        [Tooltip("Triggered when the input action is performed.")]
        public UnityEvent<InputAction.CallbackContext> InputActionPerformed;

        /// <summary>
        /// Triggered when the input action is canceled.
        /// <see
        ///     href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_canceled">
        /// InputAction.canceled
        /// Event
        /// </see>
        /// </summary>
        [Tooltip("Triggered when the input action is canceled.")]
        public UnityEvent<InputAction.CallbackContext> InputActionCanceled;

        private void OnEnable()
        {
            InputAction.action.started += InputActionStarted.Invoke;
            InputAction.action.performed += InputActionPerformed.Invoke;
            InputAction.action.canceled += InputActionCanceled.Invoke;
        }

        private void OnDisable()
        {
            InputAction.action.started -= InputActionStarted.Invoke;
            InputAction.action.performed -= InputActionPerformed.Invoke;
            InputAction.action.canceled -= InputActionCanceled.Invoke;
        }
    }
}
