// <copyright file="ReliableOnTriggerExit.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Unity does not call OnTriggerExit when a collider is being disabled.<br />
    /// ReliableOnTriggerExit ensures reliable triggering of OnTriggerExit events,
    /// even when one or both of the colliders are being disabled.
    /// This script exposes a public event delegate that will be called once
    /// when one or both of the colliders are being disabled.
    /// </summary>
    /// <remarks>
    /// Add this component during OnTriggerEnter using <see cref="ArmReliableTriggers" />
    /// to guarantee an OnTriggerExit event is fired even when one of the colliders is being
    /// disabled.<br /> <br /> After adding a <see cref="ReliableOnTriggerExit" /> you should call
    /// DisarmReliableTriggers during OnTriggerExit from at least one of the colliding objects.<br />
    /// <br /> Be aware this may cause the registered callback functions to be called during
    /// weird moments, such as when the scene is unloading, or when the application is being closed.
    /// </remarks>
    public class ReliableOnTriggerExit : MonoBehaviour
    {
        /// <summary>
        /// Callback to trigger when collision trigger exits.
        /// </summary>
        [HideInInspector]
        public UnityEvent<Collider> TriggerExitCallback = new UnityEvent<Collider>();

        /// <summary>
        /// The other reliable trigger object for this collision event.
        /// </summary>
        [HideInInspector]
        public ReliableOnTriggerExit OtherReliableOnTriggerExit;

        /// <summary>
        /// The other collider for this collision event.
        /// </summary>
        [HideInInspector]
        public Collider OtherCollider;

        /// <summary>
        /// Sets up reliable OnTriggerExit events between two colliders.<br />
        /// <br />
        /// Example usage:
        /// <br />
        /// <code>
        /// private void OnTriggerEnter(Collider other) {
        ///   // Application-specific collision handling code.
        ///   ...
        ///
        ///   // Arm the reliable trigger mechanism.
        ///   reliableOnTriggerExit =
        ///   ReliableOnTriggerExit.ArmReliableTriggers(myCollider, otherCollider,
        ///   reliableOnTriggerExitCallback);
        /// }
        /// </code>
        /// ...
        /// <code>
        /// private void OnTriggerExit(Collider otherCollider) {
        ///   // Application-specific collision handling code.
        ///   ...
        ///
        ///   // Disarm the reliable trigger mechanism.
        ///   reliableOnTriggerExit.DisarmReliableTriggers();
        /// }
        /// </code>
        /// <remarks>
        /// Calling this method will add a new <see cref="ReliableOnTriggerExit" />
        /// to both thisCollider and otherCollider gameObjects.<br />
        /// <br />
        /// At least one of the objects should call <see cref="DisarmReliableTriggers" /> inside its
        /// OnTriggerExit callback.
        /// </remarks>
        /// </summary>
        /// <param name="thisCollider">The collider on this GameObject.</param>
        /// <param name="otherCollider">The collider on the other GameObject.</param>
        /// <param name="triggerExitCallback">The callback to invoke on trigger exit.</param>
        /// <returns>New instance of ReliableOnTriggerExit attached to thisCollider.</returns>
        public static ReliableOnTriggerExit ArmReliableTriggers(
                Collider thisCollider, Collider otherCollider,
                UnityAction<Collider> triggerExitCallback)
        {
            var thisReliableOnTriggerExit =
                    thisCollider.gameObject.AddComponent<ReliableOnTriggerExit>();
            var otherReliableOnTriggerExit =
                    otherCollider.gameObject.AddComponent<ReliableOnTriggerExit>();

            thisReliableOnTriggerExit.OtherReliableOnTriggerExit = otherReliableOnTriggerExit;
            otherReliableOnTriggerExit.OtherReliableOnTriggerExit = thisReliableOnTriggerExit;

            thisReliableOnTriggerExit.OtherCollider = otherCollider;
            otherReliableOnTriggerExit.OtherCollider = thisCollider;

            thisReliableOnTriggerExit.TriggerExitCallback.AddListener(triggerExitCallback);
            otherReliableOnTriggerExit.TriggerExitCallback.AddListener(triggerExitCallback);

            return thisReliableOnTriggerExit;
        }

        /// <summary>
        /// Removes the reliable trigger component from this collider as well as from the other
        /// collider. Does not call any registered <see cref="TriggerExitCallback" />s.
        /// </summary>
        public void DisarmReliableTriggers()
        {
            TriggerExitCallback.RemoveAllListeners();
            if (OtherReliableOnTriggerExit != null)
            {
                OtherReliableOnTriggerExit.TriggerExitCallback.RemoveAllListeners();
                Destroy(OtherReliableOnTriggerExit);
            }

            Destroy(this);
        }

        // OnDisable is called before OnDestroy when destroying an Object.
        private void OnDisable()
        {
            if (OtherReliableOnTriggerExit != null)
            {
                TriggerExitCallback.Invoke(OtherReliableOnTriggerExit.OtherCollider);
            }

            DisarmReliableTriggers();
        }
    }
}
