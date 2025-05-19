// <copyright file="WiperCursor.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.ScreenWiper
{
    /// <summary>
    /// A hand cursor for painting on the sphere.
    /// </summary>
    public class WiperCursor : MonoBehaviour
    {
        [SerializeField] private Renderer _cursorRenderer;
        [SerializeField] private Vector2 _cursorShowingRadii;
        [SerializeField] private Vector2 _cursorHiddenRadii;

        [Header("Particles")]
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private float _minimumParticleEmitDuration;
        [SerializeField] private ParticleSystem _activeParticles;

        [Header("Audio")]
        [SerializeField] private AudioSource[] _audioSources;
        [SerializeField] private AudioClip[] _audioClips;
        [SerializeField] private float _audioOneShotInterval;
        [SerializeField] private float _audioStopDelay;
        [SerializeField] private Vector2 _audioPitchRange;
        [SerializeField] private Vector2 _audioVolumeRange;

        private bool _particlesEmitting;
        private bool _particlesRequestStop;
        private float _particlesEmittingTimer;

        private bool _audioRequested;
        private float _audioStopTimer;
        private int _audioIndex;
        private int _prevClipIndex;
        private float _audioTimer;

        private int _propIdOuterRad = Shader.PropertyToID("_OuterRad");
        private int _propIdInnerRad = Shader.PropertyToID("_InnerRad");

        /// <summary>Function to set the cursor's size without triggering particles.</summary>
        /// <param name="percent">Show percent, [0 : 1].</param>
        public void SetCursorShowPercentNoParticles(float percent)
        {
            float outer = Mathf.Lerp(_cursorHiddenRadii.x, _cursorShowingRadii.x, percent);
            float inner = Mathf.Lerp(_cursorHiddenRadii.y, _cursorShowingRadii.y, percent);
            _cursorRenderer.material.SetFloat(_propIdOuterRad, outer);
            _cursorRenderer.material.SetFloat(_propIdInnerRad, inner);
        }

        /// <summary>Function to set the cursor's size.</summary>
        /// <param name="percent">Show percent, [0 : 1].</param>
        public void SetCursorShowPercent(float percent)
        {
            SetCursorShowPercentNoParticles(percent);
            if (percent <= 0.0f && _activeParticles.isStopped)
            {
                _activeParticles.Play();
            }
            else if (percent > 0.0f && _activeParticles.isPlaying)
            {
                _activeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        /// <summary>Function to enable particle effects.</summary>
        /// <param name="enable">Bool to enable.</param>
        public void RequestEnableParticles(bool enable)
        {
            if (enable && !_particlesEmitting)
            {
                for (int i = 0; i < _particles.Length; ++i)
                {
                    _particles[i].Play();
                }

                _particlesEmitting = true;
                _particlesRequestStop = false;
                _particlesEmittingTimer = _minimumParticleEmitDuration;
            }
            else if (!enable && _particlesEmitting)
            {
                _particlesRequestStop = true;
            }
        }

        /// <summary>Function to enable looping audio.</summary>
        /// <param name="enable">Bool to enable.</param>
        public void RequestEnableAudio(bool enable)
        {
            _audioRequested = enable;
            if (_audioRequested)
            {
                _audioStopTimer = _audioStopDelay;
            }
        }

        private void Start()
        {
            _particlesEmittingTimer = 0.0f;
            _particlesEmitting = true;
            _particlesRequestStop = false;
            _audioRequested = false;

            RequestEnableParticles(false);
            RequestEnableAudio(false);
            _activeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void Update()
        {
            // If we requested stopping particles, don't stop out until our timer has expired.
            // More particles is more fun, so we want to always show them for a bit.
            if (_particlesRequestStop)
            {
                _particlesEmittingTimer -= Time.deltaTime;
                if (_particlesEmittingTimer <= 0.0f)
                {
                    for (int i = 0; i < _particles.Length; ++i)
                    {
                        _particles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }

                    _particlesRequestStop = false;
                    _particlesEmitting = false;
                }
            }

            _audioStopTimer -= Time.deltaTime;
            if (_audioRequested || _audioStopTimer > 0.0f)
            {
                _audioTimer -= Time.deltaTime;
                if (_audioTimer <= 0.0f)
                {
                    // Don't pick the same audio clip twice in a row.
                    int clipIndex = _prevClipIndex;
                    if (_audioClips.Length > 1)
                    {
                        while (clipIndex == _prevClipIndex)
                        {
                            clipIndex = Random.Range(0, _audioClips.Length);
                        }
                    }

                    _audioSources[_audioIndex].clip = _audioClips[clipIndex];
                    _audioSources[_audioIndex].pitch =
                        Random.Range(_audioPitchRange.x, _audioPitchRange.y);
                    _audioSources[_audioIndex].volume =
                        Random.Range(_audioVolumeRange.x, _audioVolumeRange.y);

                    _audioSources[_audioIndex].Play();
                    _prevClipIndex = clipIndex;

                    ++_audioIndex;
                    _audioIndex %= _audioSources.Length;

                    _audioTimer %= _audioOneShotInterval;
                    _audioTimer += _audioOneShotInterval;
                }
            }
            else
            {
                _audioTimer = 0.0f;
            }
        }
    }
}
