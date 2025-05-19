// <copyright file="Variable.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Variables
{
    /// <summary>
    /// Represents a generic variable that can notify listeners of its value changes.
    /// This class can be used to create observable variables of any type.
    /// </summary>
    /// <typeparam name="T">The type of the variable.</typeparam>
    public class Variable<T> : ScriptableObject
    {
        /// <summary>
        /// Backing field for the Value property. Allows setting a value from the Inspector.
        /// </summary>
        public T RawValue;

        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        /// <summary>
        /// Gets or sets the current value of the variable. Setting this value will notify all registered
        /// listeners.
        /// </summary>
        public T Value
        {
            get => RawValue;
            set
            {
                RawValue = value;
                Invoke(RawValue);
            }
        }

        /// <summary>
        /// Gets the count of currently registered listeners.
        /// </summary>
        public int ListenerCount => _listeners.Count;

        /// <summary>
        /// Invokes all registered listeners with the current value.
        /// </summary>
        /// <param name="pValue">The value to invoke the listeners with.</param>
        public void Invoke(T pValue)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke(pValue);
            }
        }

        /// <summary>
        /// Adds a new listener to be notified when the variable's value changes.
        /// </summary>
        /// <param name="action">The action to be invoked when the variable's value changes.</param>
        public void AddListener(Action<T> action)
        {
            _listeners.Add(action);
        }

        /// <summary>
        /// Removes a listener from the notification list.
        /// </summary>
        /// <param name="action">The action to remove from the listener list.</param>
        public void RemoveListener(Action<T> action)
        {
            _listeners.Remove(action);
        }
    }
}
