// <copyright file="CritterControls.cs" company="Google LLC">
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

using AndroidXRUnitySamples.SoundArena.Speakers;
using UnityEngine;
using UnityEngine.Events;

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Controller behaviour for the Critter entity.
    /// </summary>
    public class CritterControls : MonoBehaviour
    {
        private const string _kDamageParam = "TakingDamage";
        private static readonly int _damageId = Animator.StringToHash(_kDamageParam);

        [SerializeField] private CritterSpawner _spawner;
        [SerializeField] private Collider _hitbox;
        [SerializeField] private Transform _geometry;
        [SerializeField] private ParticleSystem[] _deathParticleSystems;
        [SerializeField] private float _startingHealth;
        [SerializeField] private float _healthDecayRate;
        [SerializeField] private float _deadDuration;
        [SerializeField] private RandomAudioClipPlayer _sfxPlayer;
        [SerializeField] private Vector2 _sfxInterval;
        [SerializeField] private AudioSource _damageAudioSource;
        [SerializeField] private ParticleSystem _damageSystem;

        [SerializeField] private bool _takingDamage;
        private float _currentHealth;
        private float _stateTimer;
        private float _sfxTimer;
        private ReliableOnTriggerExit _reliableOnTriggerExit;
        private UnityAction<Collider> _reliableOnTriggerExitCallback;

        private State _currentState;

        private enum State
        {
            Init,
            Spawning,
            Alive,
            Dead,
        }

        /// <summary>
        /// Sets the critter in the spawning state.
        /// </summary>
        public void Spawn()
        {
            _currentState = State.Spawning;
        }

        private void Awake()
        {
            _reliableOnTriggerExitCallback = OnTriggerExit;
            _currentState = State.Init;
            _geometry.gameObject.SetActive(false);
            _takingDamage = false;
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.Spawning:
                // Find a new spot to spawn and do it.
                transform.position = _spawner.GetRandomSpawnPosition();
                _geometry.gameObject.SetActive(true);
                _currentHealth = _startingHealth;
                _sfxTimer = 0.5f;
                _currentState = State.Alive;
                break;
            case State.Alive:
                if (_takingDamage)
                {
                    _currentHealth -= Time.deltaTime * _healthDecayRate;
                    if (_currentHealth <= 0.0f)
                    {
                        for (int i = 0; i < _deathParticleSystems.Length; ++i)
                        {
                            _deathParticleSystems[i].Play();
                        }

                        _geometry.gameObject.SetActive(false);
                        _stateTimer = _deadDuration;
                        SetTakingDamage(false);
                        _currentState = State.Dead;
                    }
                }
                else
                {
                    _sfxTimer -= Time.deltaTime;
                    if (_sfxTimer <= 0.0f)
                    {
                        _sfxPlayer.TriggerPlayback();
                        _sfxTimer = Random.Range(_sfxInterval.x, _sfxInterval.y);
                    }
                }

                break;
            case State.Dead:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0.0f)
                {
                    for (int i = 0; i < _deathParticleSystems.Length; ++i)
                    {
                        _deathParticleSystems[i].Stop();
                    }

                    _currentState = State.Spawning;
                }

                break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Flashlight"))
            {
                return;
            }

            FlashlightControls flashlight = other.GetComponentInParent<FlashlightControls>();
            if (flashlight != null && flashlight.Enabled)
            {
                SetTakingDamage(true);
            }

            _reliableOnTriggerExit = ReliableOnTriggerExit.ArmReliableTriggers(
                    _hitbox, other, _reliableOnTriggerExitCallback);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Flashlight"))
            {
                return;
            }

            SetTakingDamage(false);
            _reliableOnTriggerExit.DisarmReliableTriggers();
        }

        private void SetTakingDamage(bool takingDamage)
        {
            _takingDamage = takingDamage;
            GetComponent<Animator>().SetBool(_damageId, takingDamage);
            if (_takingDamage)
            {
                _damageSystem.Play();
                _damageAudioSource.Play();
            }
            else
            {
                _damageSystem.Stop();
                _damageAudioSource.Stop();
            }
        }
    }
}
