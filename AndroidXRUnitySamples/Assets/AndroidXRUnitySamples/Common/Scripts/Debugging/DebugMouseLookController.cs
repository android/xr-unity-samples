// <copyright file="DebugMouseLookController.cs" company="Google LLC">
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
    /// A behaviour that allows adjusting the facing angle of a transform by moving the mouse
    /// while holding a key.
    /// </summary>
    public class DebugMouseLookController : MonoBehaviour
    {
        private const string _kxAxis = "Mouse X";
        private const string _kyAxis = "Mouse Y";

        [SerializeField]
        [Range(0.1f, 9f)]
        private float _sensitivity = 2f;

        [Tooltip("Limits vertical camera rotation. Prevents the flipping "
               + "that happens when rotation goes above 90.")]
        [SerializeField]
        [Range(0f, 90f)]
        private float _rotationLimitY = 88f;

        private Vector2 _rotValue;

        private void Awake()
        {
            _rotValue = Vector2.zero;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _rotValue.x += Input.GetAxis(_kxAxis) * _sensitivity;
                _rotValue.y += Input.GetAxis(_kyAxis) * _sensitivity;
                _rotValue.y = Mathf.Clamp(_rotValue.y, -_rotationLimitY, _rotationLimitY);
                Quaternion xQuat = Quaternion.AngleAxis(_rotValue.x, Vector3.up);
                Quaternion yQuat = Quaternion.AngleAxis(_rotValue.y, Vector3.left);
                transform.localRotation = xQuat * yQuat;
            }
        }
    }
}
