// <copyright file="SoundArena.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Script for running state on the SoundArena experience.
    /// </summary>
    public class SoundArena : MonoBehaviour
    {
        [SerializeField] private CritterControls _critter;
        [SerializeField] private InteractionHint _howToHint;

        private bool _critterSpawned;

        private void Awake()
        {
            _critterSpawned = false;
        }

        private void Update()
        {
            if (!_critterSpawned && _howToHint == null)
            {
                _critter.Spawn();
                _critterSpawned = true;
            }
        }
    }
}
