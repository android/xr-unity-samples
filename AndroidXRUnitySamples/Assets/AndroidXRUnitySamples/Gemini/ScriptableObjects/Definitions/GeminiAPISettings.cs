// <copyright file="GeminiAPISettings.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Settings for the Gemini API, including the base URL and API key.
    /// This ScriptableObject can be created from the Unity menu under AndroidXRUnitySamples/Gemini/APISettings.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Gemini/APISettings")]
    public class GeminiAPISettings : ScriptableObject
    {
        /// <summary>
        /// Base URL for accessing the Gemini API.
        /// </summary>
        public string GeminiAPIUrl = string.Empty;

        /// <summary>
        /// API key for accessing the Gemini API.
        /// </summary>
        public string GeminiAPIKey = string.Empty;
    }
}
