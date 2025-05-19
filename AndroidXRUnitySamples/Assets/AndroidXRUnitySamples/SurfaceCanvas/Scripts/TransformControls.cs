// <copyright file="TransformControls.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Exposes some properties of an assigned Transform for controlling via UI events and scripts.
    /// </summary>
    public class TransformControls : MonoBehaviour
    {
        [SerializeField]
        private Transform _objectSlot;

        [SerializeField]
        private float _smallScale = .66f;

        [SerializeField]
        private float _largeScale = 1.33f;

        private Vector3 _startingPosition;
        private Vector3 _startingScale;
        private Quaternion _startingRotation;

        /// <summary>
        /// Binding for the UI Toggle control that sets the object scale to small.
        /// </summary>
        /// <param name="toggleState">The state of the toggle.</param>
        public void ScaleToggleSmall(bool toggleState)
        {
            if (toggleState)
            {
                transform.localScale *= _smallScale;
            }
        }

        /// <summary>
        /// Binding for the UI Toggle control that sets the object scale to medium.
        /// </summary>
        /// <param name="toggleState">The state of the toggle.</param>
        public void ScaleToggleMedium(bool toggleState)
        {
            if (toggleState)
            {
                transform.localScale = _startingScale;
            }
        }

        /// <summary>
        /// Binding for the UI Toggle control that sets the object scale to large.
        /// </summary>
        /// <param name="toggleState">The state of the toggle.</param>
        public void ScaleToggleLarge(bool toggleState)
        {
            if (toggleState)
            {
                transform.localScale *= _largeScale;
            }
        }

        /// <summary>
        /// Resets all parameters in the referenced Transform to their starting values.
        /// </summary>
        public void Reset()
        {
            if (_objectSlot != null)
            {
                transform.position = _objectSlot.position;
                transform.rotation = _objectSlot.rotation;
            }
            else
            {
                transform.position = _startingPosition;
                transform.rotation = _startingRotation;
            }

            transform.localScale = _startingScale;
        }

        private void Awake()
        {
            _startingPosition = transform.position;
            _startingScale = transform.localScale;
            _startingRotation = transform.rotation;
        }
    }
}
