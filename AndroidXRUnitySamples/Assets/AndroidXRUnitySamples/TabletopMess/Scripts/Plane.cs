// <copyright file="Plane.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// A XR tracked plane.
    /// </summary>
    public class Plane : MonoBehaviour
    {
        [SerializeField] private SceneState _sceneState;

        private void OnEnable()
        {
            _sceneState.PlaneTracked.Value = true;
        }

        private void OnDestroy()
        {
            _sceneState.PlaneTracked.Value = false;
        }
    }
}
