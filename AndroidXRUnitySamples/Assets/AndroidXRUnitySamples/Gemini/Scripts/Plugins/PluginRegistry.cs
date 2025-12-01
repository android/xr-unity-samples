// <copyright file="PluginRegistry.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Manages singleton instances of Plugin Bridges.
    /// Ensures bridges are properly initialized and disposed.
    /// </summary>
    public class PluginRegistry : IDisposable
    {
        private static readonly object _lock = new object();
        private static PluginRegistry _instance;
        private readonly Dictionary<Type, IDisposable> _bridges =
            new Dictionary<Type, IDisposable>();

        private bool _isDisposed;

        private PluginRegistry()
        {
        }

        ~PluginRegistry()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the singleton instance of the PluginRegistry.
        /// Creates it if it doesn't exist.
        /// </summary>
        public static PluginRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginRegistry();
                            Application.quitting += _instance.DisposeOnQuit;
                            Debug.Log("PluginRegistry initialized.");
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or creates a singleton instance of the specified Plugin Bridge type.
        /// </summary>
        /// <typeparam name="T">The type of the Plugin Bridge.</typeparam>
        /// <returns>The singleton instance of the bridge, or null if creation failed.</returns>
        public T Get<T>() where T : class, IDisposable, new()
        {
            if (_isDisposed)
            {
                Debug.LogWarning("PluginRegistry accessed after being disposed.");
                return null;
            }

            Type bridgeType = typeof(T);

            lock (_bridges)
            {
                if (_bridges.TryGetValue(bridgeType, out IDisposable existingBridge))
                {
                    return (T)existingBridge;
                }

                Debug.Log($"Creating new instance of Plugin Bridge: {bridgeType.Name}");
                try
                {
                    var newBridge = new T();
                    _bridges.Add(bridgeType, newBridge);
                    return newBridge;
                }
                catch (Exception e)
                {
                    Debug.LogError(
                            $"Failed to create instance "
                          + $"of {bridgeType.Name}: {e.Message}\n{e.StackTrace}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks if a bridge of a specific type has already been instantiated.
        /// </summary>
        /// <typeparam name="T">The type of the bridge to check.</typeparam>
        /// <returns>True if the bridge exists, false otherwise.</returns>
        public bool Has<T>() where T : class, IDisposable
        {
            if (_isDisposed)
            {
                return false;
            }

            lock (_bridges)
            {
                return _bridges.ContainsKey(typeof(T));
            }
        }

        /// <summary>
        /// Disposes a specific bridge instance managed by the registry.
        /// </summary>
        /// <typeparam name="T">The type of the bridge to dispose.</typeparam>
        public void DisposeBridge<T>() where T : class, IDisposable
        {
            if (_isDisposed)
            {
                return;
            }

            Type bridgeType = typeof(T);
            lock (_bridges)
            {
                if (_bridges.TryGetValue(bridgeType, out IDisposable bridge))
                {
                    Debug.Log($"Disposing specific bridge: {bridgeType.Name}");
                    try
                    {
                        bridge.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error disposing {bridgeType.Name}: {e.Message}");
                    }

                    _bridges.Remove(bridgeType);
                }
            }
        }

        /// <summary>
        /// Disposes all managed bridge instances.
        /// Typically called automatically on application quit or manually if needed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(),
        /// false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Debug.Log("Disposing all Plugin Bridges in Registry...");
                lock (_bridges)
                {
                    var keys = new List<Type>(_bridges.Keys);
                    foreach (Type bridgeType in keys)
                    {
                        if (_bridges.TryGetValue(bridgeType, out IDisposable bridge))
                        {
                            Debug.Log($"Disposing bridge: {bridgeType.Name}");
                            try
                            {
                                bridge.Dispose();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error "
                                             + $"disposing {bridgeType.Name}: {e.Message}");
                            }
                        }
                    }

                    _bridges.Clear();
                }

                Debug.Log("PluginRegistry disposed.");
            }

            Application.quitting -= DisposeOnQuit;
            _isDisposed = true;
            _instance = null;
        }

        private void DisposeOnQuit()
        {
            Dispose();
        }
    }
}
