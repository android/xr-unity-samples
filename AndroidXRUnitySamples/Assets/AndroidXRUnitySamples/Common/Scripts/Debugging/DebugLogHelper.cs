// <copyright file="DebugLogHelper.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// A simple behaviour that offers a hook into Unity's Debug.Log method. This can be used to
    /// quickly setup simple debug statements for UnityEvents through the inspector,
    /// during testing and debugging.
    /// </summary>
    public class DebugLogHelper : MonoBehaviour
    {
        /// <summary>
        /// Calls <see cref="Debug.Log" /> with the given string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        public void SendDebugLog(string message)
        {
            Debug.Log($"{gameObject.name} : {message}", gameObject);
        }

        /// <summary>
        /// Calls <see cref="Debug.LogWarning" /> with the given string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        public void SendDebugLogWarning(string message)
        {
            Debug.LogWarning($"{gameObject.name} : {message}", gameObject);
        }

        /// <summary>
        /// Calls <see cref="Debug.LogError" /> with the given string.
        /// </summary>
        /// <param name="message">The message string to log.</param>
        public void SendDebugLogError(string message)
        {
            Debug.LogError($"{gameObject.name} : {message}", gameObject);
        }
    }
}
