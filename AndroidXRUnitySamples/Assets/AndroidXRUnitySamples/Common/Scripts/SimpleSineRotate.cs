// <copyright file="SimpleSineRotate.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Simple procedural animation class used for rotating.
    /// </summary>
    public class SimpleSineRotate : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private Vector3 _axis;
        [SerializeField] private float _rotateAmountScalar;

        private float _value;
        private Quaternion _baseRot;

        private void Awake()
        {
            _baseRot = transform.localRotation;
        }

        private void Update()
        {
            _value += Time.deltaTime * _speed;
            Quaternion rot = Quaternion.AngleAxis(
                Mathf.Sin(_value) * _rotateAmountScalar, _axis);
            transform.localRotation = _baseRot * rot;
        }
    }
}
