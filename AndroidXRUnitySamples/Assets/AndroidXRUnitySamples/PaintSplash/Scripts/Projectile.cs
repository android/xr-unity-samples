// <copyright file="Projectile.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Class used to control the behavior of a single projectile.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const float _kMaxTravelDistance = 10;

        [SerializeField] private Transform _mesh;
        [SerializeField] private Transform _trailParent;
        [SerializeField] private float _growSpeed;
        [SerializeField] private Vector2 _squishAmount;
        [SerializeField] private Vector2 _squishSpeedRange;

        private Rigidbody _rigidBody;
        private Action _releaseAction;
        private TrailRenderer _trailRenderer;
        private Transform _launcher;
        private Vector3 _initialPosition;
        private bool _clearTrailPending;
        private float _growAmount;

        /// <summary>
        /// Accessor for getting the mesh transform.
        /// </summary>
        public Transform Mesh => _mesh;

        /// <summary>
        /// Set the action to be executed when the projectile should be recycled.
        /// </summary>
        /// <param name="action">The function to execute when the projectile is freed.</param>
        ///
        public void SetReleaseAction(Action action)
        {
            _releaseAction = action;
        }

        /// <summary>
        /// Set the launcher object for orienting at launch.
        /// </summary>
        /// <param name="launcher">The transform of our launcher.</param>
        ///
        public void SetLauncher(Transform launcher)
        {
            _launcher = launcher;
        }

        /// <summary>
        /// Launches the projectile with the given force.
        /// </summary>
        /// <param name="force">The impulse force to launch the projectile with.</param>
        /// <param name="offset">The offset of the force to apply.</param>
        public void Launch(Vector3 force, Vector3 offset)
        {
            _rigidBody.linearVelocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            _rigidBody.position = _launcher.TransformPoint(offset);
            _rigidBody.rotation = _launcher.rotation;
            Vector3 impulseForce = _launcher.rotation * force;
            _rigidBody.AddForce(impulseForce, ForceMode.Impulse);

            Quaternion lookRotation = Quaternion.LookRotation(impulseForce.normalized);
            _mesh.rotation = lookRotation;
            _trailParent.rotation = lookRotation;

            _growAmount = 0.0f;
            _mesh.localScale = Vector3.zero;

            // Delay the trail clearing to wait until projectile pose is resolved by physics.
            _clearTrailPending = true;
        }

        private void Awake()
        {
            _rigidBody = GetComponentInChildren<Rigidbody>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        private void Start()
        {
            _initialPosition = transform.position;
        }

        private void OnDestroy()
        {
            _releaseAction = null;
        }

        private void FixedUpdate()
        {
            if (_clearTrailPending)
            {
                _trailRenderer.Clear();
                _clearTrailPending = false;
            }

            // Reset if the projectile is too far from the launcher.
            if ((transform.position - _initialPosition).magnitude > _kMaxTravelDistance)
            {
                _releaseAction?.Invoke();
            }
        }

        private void Update()
        {
            _growAmount += _growSpeed * Time.deltaTime;
            _growAmount = Mathf.Min(_growAmount, 1.0f);

            float speed = _rigidBody.linearVelocity.magnitude;
            if (speed > 0.0f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(_rigidBody.linearVelocity);
                _mesh.rotation = lookRotation;
                _trailParent.rotation = lookRotation;
            }

            float squishT = Mathf.InverseLerp(_squishSpeedRange.x, _squishSpeedRange.y, speed);
            Vector3 localScale = Vector3.one;
            localScale.z = Mathf.Lerp(_squishAmount.x, _squishAmount.y, squishT);
            _mesh.localScale = localScale * _growAmount;
        }

        private void OnCollisionEnter(Collision _)
        {
            _releaseAction?.Invoke();
        }
    }
}
