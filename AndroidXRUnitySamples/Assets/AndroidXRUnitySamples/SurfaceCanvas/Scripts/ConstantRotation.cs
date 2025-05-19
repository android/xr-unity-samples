// <copyright file="ConstantRotation.cs" company="Google LLC">
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
using UnityEngine;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Simple behaviour to apply a constant rotation to a Transform.
    /// </summary>
    public class ConstantRotation : MonoBehaviour
    {
        private readonly float _speedFactor = 10f;

        [SerializeField]
        [Range(0, 10)]
        private float _speed;

        [SerializeField]
        private Vector3 _axis;

        private float _startSpeed;

        /// <summary>
        /// Toggles the rotation on or off.
        /// </summary>
        /// <param name="isOn">Whether to turn it on or off.</param>
        public void ToggleRotate(bool isOn)
        {
            _speed = Convert.ToInt32(isOn) * _startSpeed;
        }

        private void Awake()
        {
            _startSpeed = _speed;
        }

        private void Update()
        {
            transform.Rotate(_axis, _speed * _speedFactor * Time.deltaTime, Space.Self);
        }
    }
}
