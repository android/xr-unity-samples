// <copyright file="BasePluginEvent.cs" company="Google LLC">
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
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Base class for parsing the 'event' field from any plugin JSON payload.
    /// </summary>
    [Serializable]
    public class BasePluginEvent
    {
        /// <summary>
        /// Gets or sets the event type identifier.
        /// </summary>
        public string Event;

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        public long Timestamp;
    }
}
