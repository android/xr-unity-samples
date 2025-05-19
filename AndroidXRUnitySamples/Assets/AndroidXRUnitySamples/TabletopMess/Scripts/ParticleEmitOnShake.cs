// <copyright file="ParticleEmitOnShake.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// Triggers particle emission when acceleration changes above a threshold.
    /// </summary>
    public class ParticleEmitOnShake : MonoBehaviour
    {
        [SerializeField] private float _stateChangeEmitDuration;
        [SerializeField] private Vector2 _torqueTriggerRange;
        [SerializeField] private Vector2 _torqueTriggerRangeKinematic;
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private float _audioTriggerTorque;
        [SerializeField] private AudioClipData _audioClip;

        private float _emissionPreventionTimer;
        private bool _isKinematic;
        private Vector3 _prevKinematicPos;
        private Vector3 _prevLinearVelocity;
        private float _prevAccelerationMag;
        private short[] _particleBurstCounts;

        private void Start()
        {
            _isKinematic = _rigidBody.isKinematic;
            _prevKinematicPos = transform.position;
            _emissionPreventionTimer = _stateChangeEmitDuration;

            // Cache particle bursts for modification later.
            ParticleSystem.EmissionModule em = _particles.emission;
            _particleBurstCounts = new short[em.burstCount];
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[em.burstCount];
            em.GetBursts(bursts);
            for (int i = 0; i < bursts.Length; ++i)
            {
                _particleBurstCounts[i] = bursts[i].maxCount;
            }
        }

        private void FixedUpdate()
        {
            _emissionPreventionTimer -= Time.deltaTime;

            Vector3 velocity = _rigidBody.linearVelocity;
            if (_rigidBody.isKinematic)
            {
                // If our internal flag isn't kinematic, we need to initialize
                // our position so we don't get a wild jump the first frame.
                if (!_isKinematic)
                {
                    _prevKinematicPos = transform.position;
                    _isKinematic = true;

                    // On state changes, set our delay.
                    _emissionPreventionTimer = _stateChangeEmitDuration;
                }
                else
                {
                    velocity = (transform.position - _prevKinematicPos) / Time.deltaTime;
                }
            }
            else
            {
                if (_isKinematic)
                {
                    // On state changes, set our delay.
                    _emissionPreventionTimer = _stateChangeEmitDuration;
                }

                _isKinematic = false;
            }

            Vector3 accel = (velocity - _prevLinearVelocity) / Time.deltaTime;
            _prevLinearVelocity = velocity;

            float accelMag = accel.magnitude;
            float torque = (accelMag - _prevAccelerationMag) / Time.deltaTime;
            _prevAccelerationMag = accelMag;

            Vector2 emitRange = _isKinematic ?
                _torqueTriggerRangeKinematic : _torqueTriggerRange;
            if (torque > emitRange.x && _emissionPreventionTimer <= 0.0f)
            {
                // If we're moving fast enough, trigger audio.
                if (torque > _audioTriggerTorque)
                {
                    Singleton.Instance.Audio.PlayOneShot(_audioClip, transform.position);
                }

                // Depending on how fast we moved, we want to emit a percentage of our
                // total particles per burst.
                float emitPercent = Mathf.InverseLerp(emitRange.x, emitRange.y, torque);

                ParticleSystem.EmissionModule em = _particles.emission;
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[em.burstCount];
                em.GetBursts(bursts);
                for (int i = 0; i < bursts.Length; ++i)
                {
                    bursts[i].maxCount = (short)((float)_particleBurstCounts[i] * emitPercent);
                }

                em.SetBursts(bursts);

                _particles.Play();
            }
        }
    }
}
