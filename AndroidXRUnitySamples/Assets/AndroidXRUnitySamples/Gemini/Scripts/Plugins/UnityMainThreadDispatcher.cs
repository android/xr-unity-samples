// <copyright file="UnityMainThreadDispatcher.cs" company="Google LLC">
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
using System.Collections.Concurrent;
using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Executes actions on the Unity main thread.
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly object _instanceLock = new object();
        private static UnityMainThreadDispatcher _instance;
        private static bool _isInitialized;

        private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Gets the singleton instance. Returns null if accessed before initialization.
        /// </summary>
        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (!_isInitialized && _instance == null)
                {
                    Debug.LogWarning(
                            "UnityMainThreadDispatcher accessed before initialization "
                          + "is complete. This might happen if accessed from another "
                          + "RuntimeInitializeOnLoad method with earlier execution order.");
                }

                return _instance;
            }
        }

        /// <summary>
        /// Queues an action to be executed on the main thread during the next Update cycle.
        /// Safe to call from any thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void RunOnMainThread(Action action)
        {
            if (Instance != null)
            {
                Instance.Enqueue(action);
            }
            else
            {
                Debug.LogError(
                        "UnityMainThreadDispatcher.Instance is null. "
                      + "Cannot run action. Was it accessed before "
                      + "initialization or during shutdown?");
            }
        }

        /// <summary>
        /// Queues an action to be executed on the main thread during the next Update cycle.
        /// Safe to call from any thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void Enqueue(Action action)
        {
            if (action != null)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private void Awake()
        {
            lock (_instanceLock)
            {
                if (_instance != null && _instance != this)
                {
                    Debug.LogWarning(
                            $"Duplicate UnityMainThreadDispatcher detected on "
                          + $"GameObject '{gameObject.name}'. Destroying this duplicate instance.");
                    Destroy(gameObject);
                    return;
                }

                if (_instance == null)
                {
                    _instance = this;
                }

                _isInitialized = true;

                Debug.Log($"UnityMainThreadDispatcher Awake() "
                        + $"on GameObject '{gameObject.name}'.");
            }
        }

        private void Update()
        {
            while (_executionQueue.TryDequeue(out Action action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(
                            $"Error executing action via "
                          + $"UnityMainThreadDispatcher: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void OnDestroy()
        {
            lock (_instanceLock)
            {
                if (_instance == this)
                {
                    _instance = null;
                    _isInitialized = false;
                    Debug.Log("UnityMainThreadDispatcher instance destroyed.");
                }
            }
        }
    }
}
