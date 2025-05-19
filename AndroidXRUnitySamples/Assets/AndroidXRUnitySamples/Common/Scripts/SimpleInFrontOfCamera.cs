// <copyright file="SimpleInFrontOfCamera.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Simple behavior for keeping an object in front of the camera.
    /// </summary>
    public class SimpleInFrontOfCamera : MonoBehaviour
    {
        [SerializeField] private float _forwardOffset;
        [SerializeField] private float _verticalOffset;
        [SerializeField] private bool _allowPitch;
        [SerializeField] private float _oneEuroFilterBeta;
        [SerializeField] private float _oneEuroFilterMinCutoff;

        private OneEuroFilterVector3 _oneEuroFilterTargetPos;

        private void Start()
        {
            _oneEuroFilterTargetPos = new OneEuroFilterVector3()
            {
                Beta = _oneEuroFilterBeta,
                MinCutoff = _oneEuroFilterMinCutoff
            };

            transform.position = CalculateTargetPos();
            _oneEuroFilterTargetPos.Init(transform.position);
        }

        private void Update()
        {
#if UNITY_EDITOR
            // For debugging. No need to update these every frame.
            _oneEuroFilterTargetPos.Beta = _oneEuroFilterBeta;
            _oneEuroFilterTargetPos.MinCutoff = _oneEuroFilterMinCutoff;
#endif

            Transform camXf = Singleton.Instance.Camera.transform;
            Vector3 desiredPos = CalculateTargetPos();
            transform.position = _oneEuroFilterTargetPos.Step(Time.time, desiredPos);
            transform.forward = (transform.position - camXf.position).normalized;
        }

        private Vector3 CalculateTargetPos()
        {
            Transform camXf = Singleton.Instance.Camera.transform;
            Vector3 fwd = camXf.forward;
            if (!_allowPitch)
            {
                fwd.y = 0.0f;
            }

            Vector3 returnPos = camXf.position + fwd.normalized * _forwardOffset;
            returnPos.y += _verticalOffset;
            return returnPos;
        }
    }
}
