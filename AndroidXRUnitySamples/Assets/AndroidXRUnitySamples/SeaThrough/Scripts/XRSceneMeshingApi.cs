// <copyright file="XRSceneMeshingApi.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Temporary hack to disable scene meshing using reflection.
    /// </summary>
    public static class XRSceneMeshingApi
    {
        // 1. Caching the Assembly and MethodInfo for performance
        private static readonly Assembly _extensionsAssembly;
        private static readonly MethodInfo _setEnabledMethod;

        // Static constructor to initialize the cached members
        static XRSceneMeshingApi()
        {
            try
            {
                // Cache the assembly. This assumes the assembly is named "Google.XR.Extensions".
                // Adjust the name if necessary.
                _extensionsAssembly = Assembly.Load("Google.XR.Extensions");

                if (_extensionsAssembly == null)
                {
                    Debug.LogError(
                        "XRSceneMeshingApi: Failed to load Google.XR.Extensions assembly.");
                    return;
                }

                // Get the internal type from the cached assembly
                Type xrSceneMeshingApiType = _extensionsAssembly.GetType(
                    "Google.XR.Extensions.Internal.XRSceneMeshingApi");

                if (xrSceneMeshingApiType != null)
                {
                    // Cache the MethodInfo using the appropriate binding flags
                    _setEnabledMethod = xrSceneMeshingApiType.GetMethod(
                        "SetEnabled",
                        BindingFlags.Static | BindingFlags.Public);

                    if (_setEnabledMethod == null)
                    {
                        Debug.LogError("XRSceneMeshingApi: Could not find the SetEnabled method.");
                    }
                }
                else
                {
                    Debug.LogError("XRSceneMeshingApi: Could not find the XRSceneMeshingApi type.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "XRSceneMeshingApi: An exception occurred during static initialization:" +
                    $"{ex.Message}");
            }
        }

        /// <summary>
        /// Uses reflection to call the internal SetEnabled method from Google.XR.Extensions.
        /// </summary>
        /// <param name="enabled">Whether scene meshing should be enabled.</param>
        public static void SetEnabled(bool enabled)
        {
            // 2. Check if the reflection setup was successful
            if (_setEnabledMethod == null)
            {
                Debug.LogError("XRSceneMeshingAPI: SetEnabled method is not available due to a" +
                    " reflection error.");
                return;
            }

            try
            {
                // 3. Prepare parameters for the reflective call
                object[] parameters = new object[] { enabled };

                // 4. Invoke the method using the cached MethodInfo
                // The first argument is null because it is a static method.
                _setEnabledMethod.Invoke(null, parameters);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError("XRSceneMeshingAPI: An exception occurred while invoking" +
                    $" SetEnabled: {ex.Message}");
                return;
            }
        }
    }
}
