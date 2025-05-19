// <copyright file="Session.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Session information for the current user.
    /// <para />
    /// Only one instance of this asset should exist in the project.
    /// <para />
    /// <see cref="CurrentRoom" />
    /// Components interested in the scene loading state should reference that single instance
    /// in order to subscribe to loading state changes.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Common/Session")]
    public class Session : ScriptableObject
    {
        /// <summary>
        /// Reference to the <see cref="AndroidXRUnitySamples.SceneLoadingState" />.
        /// </summary>
        public SceneLoadingState SceneLoadingState;

        /// <summary>
        /// Setting which defines when debug mode is enabled to display extra info on screen.
        /// </summary>
        public BoolVariable DebugMode;
    }
}
