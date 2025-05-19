// <copyright file="SceneLoadingState.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Allows subscribing to the scene loading state. See <see cref="LoadingState" />.
    /// <para />
    /// The scene loading state is updated by <see cref="SceneManager" />.
    /// <para />
    /// Only one instance of this asset should exist in the project. This instance is then
    /// updated by <see cref="SceneManager" />.
    /// <para />
    /// Components interested in the scene loading state should reference that single instance
    /// in order to subscribe to loading state changes.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Common/SceneLoadingState")]
    public class SceneLoadingState : Variable<LoadingState>
    {
    }

    /// <summary>
    /// Encapsulates properties related to the scene loading state.
    /// </summary>
    [Serializable]
    public class LoadingState
    {
        /// <summary>
        /// Is scene loading in progress.
        /// </summary>
        public bool IsLoading;

        /// <summary>
        /// Progress of the currently running scene loading operation.
        /// </summary>
        [Range(0f, 1f)]
        public float LoadProgress;
    }
}
