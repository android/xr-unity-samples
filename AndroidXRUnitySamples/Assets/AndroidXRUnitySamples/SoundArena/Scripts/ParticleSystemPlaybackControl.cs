// <copyright file="ParticleSystemPlaybackControl.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena.Animation
{
    /// <summary>
    /// Allows controlling a ParticleSystem's playback state from an animation clip.
    /// </summary>
    /// <remarks>
    /// Animation clips can start and stop a ParticleSystem by changing the isPlaying boolean.
    /// </remarks>
    public class ParticleSystemPlaybackControl : MonoBehaviour
    {
        /// <summary>
        /// Flag to control the ParticleSystem's playback state via animation clips.
        /// </summary>
        /// <remarks>
        /// When set to true, the ParticleSystem will start playing if it is not already playing.
        /// When set to false, the ParticleSystem will stop if it is playing.
        /// This happens during LateUpdate.
        /// </remarks>
        [Tooltip(
                "Flag to control the ParticleSystem's playback state via animation clips.\n "
              + "When set to true, the ParticleSystem will start playing "
              + "if it is not already playing.\n "
              + "When set to false, the ParticleSystem will stop if it is playing.\n "
              + "This happens during LateUpdate.")]
        public bool IsPlaying;

        /// <summary>
        /// The target ParticleSystems to control.
        /// </summary>
        [SerializeField]
        private ParticleSystem[] _targetParticleSystems;

        private void LateUpdate()
        {
            if (_targetParticleSystems.Length > 0)
            {
                // Use _targetParticleSystems[0] as proxy.
                if (IsPlaying && !_targetParticleSystems[0].isPlaying)
                {
                    for (int i = 0; i < _targetParticleSystems.Length; ++i)
                    {
                        _targetParticleSystems[i].Play();
                    }
                }

                if (!IsPlaying && _targetParticleSystems[0].isPlaying)
                {
                    for (int i = 0; i < _targetParticleSystems.Length; ++i)
                    {
                        _targetParticleSystems[i].Stop();
                    }
                }
            }
        }
    }
}
