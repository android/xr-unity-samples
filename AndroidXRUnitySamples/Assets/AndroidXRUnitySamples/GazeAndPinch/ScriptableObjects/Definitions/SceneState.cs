// <copyright file="SceneState.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used to represent the scene's state.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/GazeAndPinch/SceneState")]
    public class SceneState : ScriptableObject
    {
        /// <summary>Phase of the scene.</summary>
        public ScenePhase Phase;

        /// <summary>Plane used for hosting the scene.</summary>
        public ScenePlane ScenePlane;
    }
}
