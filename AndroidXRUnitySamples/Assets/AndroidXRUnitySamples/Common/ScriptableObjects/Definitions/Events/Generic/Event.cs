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

namespace AndroidXRUnitySamples.Events.Generic
{
    /// <summary>
    /// Parameterized event class for handling events. Accepts one parameter of type
    /// <typeparamref name="T" />
    /// . Extends ScriptableObject so that it can be managed as an asset within the
    /// project, and be referenced directly without being attached to a scene object.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to listeners on invocation.</typeparam>
    public class Event<T> : ScriptableObject
    {
        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        /// <summary>
        /// Gets the count of registered listeners.
        /// </summary>
        public int ListenerCount => _listeners.Count;

        /// <summary>
        /// Invokes all registered listeners with the provided value, in reverse order of their
        /// addition.
        /// </summary>
        /// <param name="value">The value to pass to each listener.</param>
        public void Invoke(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke(value);
            }
        }

        /// <summary>
        /// Adds a listener to the event.
        /// </summary>
        /// <param name="action">The Action to add.</param>
        public void AddListener(Action<T> action)
        {
            _listeners.Add(action);
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="action">The Action to remove.</param>
        public void RemoveListener(Action<T> action)
        {
            _listeners.Remove(action);
        }
    }
}
