// <copyright file="DronePhysics.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.Drone
{
    /// <summary>
    /// Handles inputs and physics for the drone object.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DronePhysics : MonoBehaviour
    {
        private const string _kCollidedParam = "Collided";
        private static readonly int _collidedParamId = Animator.StringToHash(_kCollidedParam);

        [Header("Objects")]
        [SerializeField] private ConstantForce _force;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Animator _animator;

        [Header("Controls")]
        [SerializeField] private float _altitudeAccelerationFactor = 1.0f;
        [SerializeField] private float _directionAccelerationFactor = 1.0f;
        [SerializeField] private float _yawRotationSpeed = 1.0f;
        [SerializeField] private float _constantUpForce = 1.0f;

        [Header("Prefabs")]
        [SerializeField] private GameObject _nonfracturedDrone;
        [SerializeField] private GameObject _fracturedDronePrefab;
        [SerializeField] private GameObject _spawnDronePrefab;

        [Header("Input")]
        [SerializeField] private InputActionReference _altitudeControl;
        [SerializeField] private InputActionReference _directionControl;
        [SerializeField] private InputActionReference _yawControl;

        [Header("Explosion")]
        [SerializeField] private float _minExplosionForce = 0.0f;
        [SerializeField] private float _maxExplosionForce = 100.0f;
        [SerializeField] private float _explosionForceRadius = 1.0f;
        [SerializeField] private float _explosionForceUpwardsModifier = 0.1f;
        [SerializeField] private GameObject[] _explosionParticleSystems;

        [Header("Reset")]
        [SerializeField] private float _spawnDistance;
        [SerializeField] private float _showSpawnDroneDelay;

        private GameObject _fracturedDrone;
        private Rigidbody[] _fracturedDroneRigidbodies;
        private GameObject _spawnDrone;
        private float _showSpawnDroneTimer;

        /// <summary>
        /// Resets the drone to a position in front of the camera.
        /// </summary>
        public void ResetDrone()
        {
            _animator.SetBool(_collidedParamId, false);

            // Place the drone in front of the camera.
            transform.position = Camera.main.transform.position +
                Camera.main.transform.forward * _spawnDistance;

            // Set the yaw to match the camera.
            transform.rotation = Quaternion.AngleAxis(
                Camera.main.transform.eulerAngles.y, Vector3.up);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _force.force = Vector3.zero;
            _nonfracturedDrone.SetActive(true);

            if (_fracturedDrone != null)
            {
                Destroy(_fracturedDrone);
            }

            _fracturedDrone = Instantiate(_fracturedDronePrefab, transform);
            _fracturedDroneRigidbodies = _fracturedDrone.GetComponentsInChildren<Rigidbody>();
            _fracturedDrone.SetActive(false);

            _spawnDrone.SetActive(false);
        }

        private void Start()
        {
            if (_force == null)
            {
                _force = GetComponent<ConstantForce>();
            }

            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            _nonfracturedDrone.SetActive(false);
            _spawnDrone = Instantiate(_spawnDronePrefab);
            _spawnDrone.SetActive(false);
        }

        private void UpdateFlyingDrone()
        {
            // Set constant up force to counter the spring down force.
            _force.force = _constantUpForce * Vector3.up;

            _force.force += _altitudeAccelerationFactor *
                            _altitudeControl.action.ReadValue<float>() * Vector3.up;

            Vector2 directionInput = _directionControl.action.ReadValue<Vector2>();
            Vector3 directionForce =
                _directionAccelerationFactor * directionInput.x * Vector3.right +
                _directionAccelerationFactor * directionInput.y * Vector3.forward;

            // Only apply yaw to the direction force, do not apply pitch or roll.
            _force.force +=
                Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * directionForce;

            float yawRotationAmount =
                _yawRotationSpeed * _yawControl.action.ReadValue<float>() * Time.deltaTime;
            transform.Rotate(Vector3.up, yawRotationAmount);
        }

        private void Update()
        {
            if (!_animator.GetBool(_collidedParamId))
            {
                UpdateFlyingDrone();
            }

            if (_showSpawnDroneTimer > 0.0f)
            {
                _showSpawnDroneTimer -= Time.deltaTime;
                if (_showSpawnDroneTimer <= 0.0f)
                {
                    _spawnDrone.SetActive(true);
                }
            }

            // Keep spawn drone loocked at our spawn point.
            if (_spawnDrone.activeSelf)
            {
                _spawnDrone.transform.position = Camera.main.transform.position +
                    Camera.main.transform.forward * _spawnDistance;
                _spawnDrone.transform.rotation = Quaternion.AngleAxis(
                    Camera.main.transform.eulerAngles.y, Vector3.up);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Ignore collision callbacks if we're disabled.
            if (!_nonfracturedDrone.activeSelf)
            {
                return;
            }

            Collider firstCollider = collision.GetContact(0).thisCollider;
            if (firstCollider.gameObject == _nonfracturedDrone ||
                firstCollider.gameObject.transform.parent.gameObject == _nonfracturedDrone)
            {
                _animator.SetBool(_collidedParamId, true);
                _force.force = Vector3.zero;
                _nonfracturedDrone.SetActive(false);
                _fracturedDrone.SetActive(true);
                ExplodeFracturedDrone();
                _showSpawnDroneTimer = _showSpawnDroneDelay;
            }
        }

        private void ExplodeFracturedDrone()
        {
            for (int i = 0; i < _explosionParticleSystems.Length; ++i)
            {
                Instantiate(_explosionParticleSystems[i], transform.position, transform.rotation);
            }

            foreach (Rigidbody rb in _fracturedDroneRigidbodies)
            {
                float force = Random.Range(_minExplosionForce, _maxExplosionForce);
                rb.AddExplosionForce(force, _fracturedDrone.transform.position,
                                    _explosionForceRadius, _explosionForceUpwardsModifier);
            }
        }
    }
}
