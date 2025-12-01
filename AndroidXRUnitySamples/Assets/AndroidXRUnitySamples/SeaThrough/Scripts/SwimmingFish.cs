// <copyright file="SwimmingFish.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Class attached to each fish object responsible to handling the swimming motion and shader
    /// effects.
    /// </summary>
    public class SwimmingFish : MonoBehaviour
    {
        /// <summary>
        /// Swimming speed in meters per second.
        /// </summary>
        public float SwimmingSpeed = 0.2f;

        /// <summary>
        /// Speed multiplier when the fish is swimming away from the user.
        /// </summary>
        public float SwimmingAwaySpeedMultipiler = 2.0f;

        /// <summary>
        /// Left hand game object.
        /// </summary>
        public GameObject LeftHand;

        /// <summary>
        /// Right hand game object.
        /// </summary>
        public GameObject RightHand;

        private static readonly float _distanceToHandFarThreshold = 1.5f;

        private static readonly int _swimmingSpeedProperty = Shader.PropertyToID("_SwimmingSpeed");
        private Dictionary<Transform, MeshRenderer> _meshRenderers =
            new Dictionary<Transform, MeshRenderer>();

        private SwimmingState _swimmingState = SwimmingState.Normal;
        private GameObject _lastCollidedGameobject = null;
        private float _lastSwimmingSpeedMultiplier = 1.0f;
        private MaterialPropertyBlock _materialPropertyBlock = null;

        private enum SwimmingState
        {
            Normal, SwimmingAway
        }

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    _meshRenderers.Add(child, meshRenderer);
                }
            }
        }

        private void Update()
        {
            if (_swimmingState == SwimmingState.SwimmingAway && _lastCollidedGameobject != null)
            {
                float distanceToLastCollidedGameobject = Vector3.Distance(
                    transform.position, _lastCollidedGameobject.transform.position);
                if (distanceToLastCollidedGameobject > _distanceToHandFarThreshold)
                {
                    _swimmingState = SwimmingState.Normal;
                }
            }

            float swimmingSpeedMultiplier = _swimmingState == SwimmingState.Normal ? 1.0f :
                SwimmingAwaySpeedMultipiler;
            transform.position += Time.deltaTime * SwimmingAwaySpeedMultipiler * SwimmingSpeed *
                (transform.rotation * Vector3.right);

            if (!Mathf.Approximately(_lastSwimmingSpeedMultiplier, swimmingSpeedMultiplier))
            {
                foreach (var (_, renderer) in _meshRenderers)
                {
                    _materialPropertyBlock.Clear();
                    renderer.GetPropertyBlock(_materialPropertyBlock);
                    _materialPropertyBlock.SetFloat(
                        _swimmingSpeedProperty, swimmingSpeedMultiplier);
                    renderer.SetPropertyBlock(_materialPropertyBlock);
                }

                _lastSwimmingSpeedMultiplier = swimmingSpeedMultiplier;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject == LeftHand ||
                collision.collider.gameObject == RightHand)
            {
                _swimmingState = SwimmingState.SwimmingAway;
                Vector3 handToFish = transform.position - collision.transform.position;

                // Rotate the fish away from the hand.
                transform.rotation = Quaternion.AngleAxis(
                    Mathf.Atan2(handToFish.z, handToFish.x),
                    Vector3.up);
                _lastCollidedGameobject = collision.collider.gameObject;
            }
        }
    }
}
