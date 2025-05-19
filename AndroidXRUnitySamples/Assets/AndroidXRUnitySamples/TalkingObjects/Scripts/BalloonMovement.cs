// <copyright file="BalloonMovement.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TalkingObjects
{
    /// <summary>
    /// Movement for the balloon puppet.
    /// </summary>
    public class BalloonMovement : MonoBehaviour
    {
        [SerializeField] private float _bobScale;
        [SerializeField] private float _bobSpeed;
        [SerializeField] private float _rotateScale;
        [SerializeField] private float _rotateSpeed;

        private Vector3 _baseLocalPos;
        private Vector3 _baseLocalEulers;
        private float _t;

        private void Awake()
        {
            _baseLocalPos = transform.localPosition;
            _baseLocalEulers = transform.localEulerAngles;
        }

        private void Update()
        {
            _t += Time.deltaTime;

            Vector3 localPos = _baseLocalPos;
            localPos.y += Mathf.Sin(_t * _bobSpeed) * _bobScale;
            transform.localPosition = localPos;

            Vector3 localEulers = _baseLocalEulers;
            localEulers.y += Mathf.Sin(_t * _rotateSpeed) * _rotateScale;
            transform.localEulerAngles = localEulers;
        }
    }
}
