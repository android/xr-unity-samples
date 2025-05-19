// <copyright file="TransformOneEuroFilterPostProcessor.cs" company="Google LLC">
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

using AndroidXRUnitySamples.InputDevices;
using UnityEngine;

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// A post processor using the One Euro filter to smooth transform positions.
    /// </summary>
    public class TransformOneEuroFilterPostProcessor : MonoBehaviour
    {
        private OneEuroFilterVector3 _oneEuroFilterTargetPos;

        [SerializeField]
        [Tooltip("Smoothing amount at low speeds.")]
        private float _oneEuroFilterMinCutoff = 0.1f;

        [SerializeField]
        [Tooltip("Filter's responsiveness to speed changes.")]
        private float _oneEuroFilterBeta = 0.2f;

        private void OnEnable()
        {
            _oneEuroFilterTargetPos = new OneEuroFilterVector3()
            {
                    Beta = _oneEuroFilterBeta,
                    MinCutoff = _oneEuroFilterMinCutoff
            };

            _oneEuroFilterTargetPos.Init(transform.position);
        }

        private void LateUpdate()
        {
            Vector3 newPosition = _oneEuroFilterTargetPos.Step(Time.time, transform.position);
            transform.position = newPosition;
        }
    }
}
