// <copyright file="OneEuroFilterVector3.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.InputDevices
{
    /// <summary>
    /// A class for smoothing a Vector3 with a One Euro filter.
    /// The OneEuro filter responds to quick movements while simultaneously
    /// dampening small jitters. Overtuning can cause laggy movement and undertuning
    /// can make the filter ineffective, as if no filter was applied at all.
    /// </summary>
    public class OneEuroFilterVector3
    {
        /// <summary>Smoothing value.</summary>
        public float Beta;

        /// <summary>Lower bound noise cutoff.</summary>
        public float MinCutoff;

        const float _kDCutOff = 1.0f;
        private float _previousTime;
        private Vector3 _previousTarget;
        private Vector3 _previousFilteredTarget;

        /// <summary>Sets previous target to prevent initial jumps.</summary>
        /// <param name="targetVec3">Target value.</param>
        public void Init(Vector3 targetVec3)
        {
            _previousTarget = targetVec3;
        }

        /// <summary>Advances filter calculation with a new value.</summary>
        /// <param name="time">Current time of application.</param>
        /// <param name="targetVec3">Target value.</param>
        /// <returns>Returns the filtered value.</returns>
        public Vector3 Step(float time, Vector3 targetVec3)
        {
            float timeElapsed = time - _previousTime;

            // Do nothing if the time difference is too small.
            if (timeElapsed < 1e-5f)
            {
                return _previousTarget;
            }

            Vector3 movement = (targetVec3 - _previousTarget) / timeElapsed;
            Vector3 filteredMovement =
                Vector3.Lerp(_previousFilteredTarget, movement, Alpha(timeElapsed, _kDCutOff));

            float cutoff = MinCutoff + Beta * filteredMovement.magnitude;
            Vector3 newResult =
                Vector3.Lerp(_previousTarget, targetVec3, Alpha(timeElapsed, cutoff));

            _previousTime = time;
            _previousTarget = newResult;
            _previousFilteredTarget = filteredMovement;

            return newResult;
        }

        float Alpha(float timeElapsed, float cutoff)
        {
            float r = 2.0f * Mathf.PI * cutoff * timeElapsed;
            return r / (r + 1.0f);
        }
    }
}
