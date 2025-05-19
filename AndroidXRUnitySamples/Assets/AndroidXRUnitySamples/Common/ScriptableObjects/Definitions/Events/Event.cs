// <copyright file="Event.cs" company="Google LLC">
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
using UnityEngine;

namespace AndroidXRUnitySamples.Events
{
    /// <summary>
    /// Parameterless event class for handling events.
    /// Extends ScriptableObject so that it can be managed as an asset within the project,
    /// and be referenced directly without being attached to a scene object.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Common/Events/Event")]
    public class Event : ScriptableObject
    {
        private readonly List<Action> _listeners = new List<Action>();

        /// <summary>
        /// Gets the count of registered listeners.
        /// </summary>
        public int ListenerCount => _listeners.Count;

        /// <summary>
        /// Invokes all registered listeners in reverse order of their addition.
        /// </summary>
        public void Invoke()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke();
            }
        }

        /// <summary>
        /// Adds a listener to the event.
        /// </summary>
        /// <param name="action">The Action to add.</param>
        /// <param name="invokeOnce">
        /// Whether to unsubscribe the action after invoking once. Default
        /// false.
        /// </param>
        public void AddListener(Action action, bool invokeOnce = false)
        {
            Action invokeOnceAction = null;
            if (invokeOnce)
            {
                invokeOnceAction = () =>
                {
                    action();
                    RemoveListener(invokeOnceAction);
                };
            }

            _listeners.Add(invokeOnce ? invokeOnceAction : action);
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="lis">The Action to remove.</param>
        public void RemoveListener(Action lis)
        {
            _listeners.Remove(lis);
        }
    }
}
